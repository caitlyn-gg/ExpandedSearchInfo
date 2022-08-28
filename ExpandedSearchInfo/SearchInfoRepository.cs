using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using ExpandedSearchInfo.Providers;
using ExpandedSearchInfo.Sections;
using Nager.PublicSuffix;

namespace ExpandedSearchInfo {
    public class ExpandedSearchInfo {
        public string Info { get; }
        public List<ISearchInfoSection> Sections { get; }

        public ExpandedSearchInfo(string info, List<ISearchInfoSection> sections) {
            this.Info = info;
            this.Sections = sections;
        }
    }

    public class SearchInfoRepository : IDisposable {
        private Plugin Plugin { get; }
        private DomainParser Parser { get; }
        internal ConcurrentDictionary<uint, ExpandedSearchInfo> SearchInfos { get; } = new();
        internal uint LastObjectId { get; private set; }

        private List<IProvider> Providers { get; } = new();
        internal IEnumerable<IProvider> AllProviders => this.Providers;

        internal SearchInfoRepository(Plugin plugin) {
            this.Plugin = plugin;

            // create the public suffix list parser
            var provider = new WebTldRuleProvider();
            if (!provider.CacheProvider.IsCacheValid()) {
                provider.BuildAsync().GetAwaiter().GetResult();
            }

            this.Parser = new DomainParser(provider);

            // add providers
            this.AddProviders();

            // listen for search info
            this.Plugin.Functions.ReceiveSearchInfo += this.ProcessSearchInfo;
        }

        public void Dispose() {
            this.Plugin.Functions.ReceiveSearchInfo -= this.ProcessSearchInfo;
        }

        private void AddProviders() {
            this.Providers.Add(new PastebinProvider(this.Plugin));
            this.Providers.Add(new CarrdProvider(this.Plugin));
            this.Providers.Add(new FListProvider(this.Plugin));
            this.Providers.Add(new RefsheetProvider(this.Plugin));
            this.Providers.Add(new PlainTextProvider(this.Plugin));
        }

        private void ProcessSearchInfo(uint objectId, SeString raw) {
            this.LastObjectId = objectId;

            var info = raw.TextValue;

            // if empty search info, short circuit
            if (string.IsNullOrWhiteSpace(info)) {
                // remove any existing search info
                this.SearchInfos.TryRemove(objectId, out _);
                return;
            }

            // check to see if info has changed
            #if RELEASE
            if (this.SearchInfos.TryGetValue(objectId, out var existing)) {
                if (existing.Info == info) {
                    return;
                }
            }
            #endif

            Task.Run(async () => {
                try {
                    await this.DoExtraction(objectId, info);
                } catch (Exception ex) {
                    PluginLog.LogError($"Error in extraction thread:\n{ex}");
                }
            });
        }

        private async Task DoExtraction(uint objectId, string info) {
            var downloadUris = new List<Uri>();

            // extract uris from the search info with providers
            var extractedUris = this.Providers
                .Where(provider => provider.Config.Enabled && provider.ExtractsUris)
                .Select(provider => provider.ExtractUris(objectId, info))
                .Where(uris => uris != null)
                .SelectMany(uris => uris!);

            // add the extracted uris to the list
            downloadUris.AddRange(extractedUris);

            // go word-by-word and try to parse a uri
            foreach (var word in info.Split(' ', '\n', '\r')) {
                Uri found;
                try {
                    found = new UriBuilder(word.Trim()).Uri;
                } catch (UriFormatException) {
                    continue;
                }

                // make sure the hostname is a valid domain
                try {
                    if (!this.Parser.IsValidDomain(found.Host)) {
                        continue;
                    }
                } catch (ParseException) {
                    continue;
                }

                downloadUris.Add(found);
            }

            // if there were no uris found or extracted, remove existing search info and stop
            if (downloadUris.Count == 0) {
                this.SearchInfos.TryRemove(objectId, out _);
                return;
            }

            // do the downloads
            await this.DownloadAndExtract(objectId, info, downloadUris);
        }

        private async Task DownloadAndExtract(uint objectId, string info, IEnumerable<Uri> uris) {
            var handler = new HttpClientHandler {
                UseCookies = true,
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5,
            };
            handler.CookieContainer.Add(new Cookie("warning", "1", "/", "www.f-list.net"));

            var version = this.GetType().Assembly.GetName().Version?.ToString(3) ?? "unknown";
            var client = new HttpClient(handler) {
                DefaultRequestHeaders = {
                    UserAgent = { new ProductInfoHeaderValue("ExpandedSearchInfo", version) },
                },
            };

            var sections = new List<ISearchInfoSection>();

            // run through each extracted uri
            foreach (var uri in uris) {
                if (uri.Scheme is not ("http" or "https")) {
                    continue;
                }

                // find the providers that run on this uri
                var matching = this.Providers
                    .Where(provider => provider.Config.Enabled && provider.Matches(uri))
                    .ToList();

                // skip the uri if no providers
                if (matching.Count == 0) {
                    continue;
                }

                // get the http response from the uri and make sure it's ok
                var req = new HttpRequestMessage(HttpMethod.Get, uri);
                foreach (var provider in matching) {
                    provider.ModifyRequest(req);
                }

                var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
                if (resp.StatusCode != HttpStatusCode.OK) {
                    continue;
                }

                // go through each provider in order and take the first one that provides info
                foreach (var provider in matching) {
                    var extracted = await provider.ExtractInfo(resp);
                    if (extracted == null) {
                        continue;
                    }

                    sections.Add(extracted);
                    break;
                }
            }

            // remove expanded search info if no sections resulted
            if (sections.Count == 0) {
                this.SearchInfos.TryRemove(objectId, out _);
                return;
            }

            // otherwise set the expanded search info for this actor id
            this.SearchInfos[objectId] = new ExpandedSearchInfo(info, sections);
        }
    }
}
