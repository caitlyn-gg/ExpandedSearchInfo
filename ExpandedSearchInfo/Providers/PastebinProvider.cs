using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExpandedSearchInfo.Configs;
using ExpandedSearchInfo.Sections;

namespace ExpandedSearchInfo.Providers {
    public class PastebinProvider : IProvider {
        private static readonly Regex Matcher = new(@"pb:(\S+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private Plugin Plugin { get; }

        public string Name => "pastebin.com";

        public string Description => "This provider provides information from pastebin.com URLs. It also works on tags such as \"pb:pasteid\".";

        public BaseConfig Config => this.Plugin.Config.Configs.Pastebin;

        public bool ExtractsUris => true;

        public PastebinProvider(Plugin plugin) {
            this.Plugin = plugin;
        }

        public void DrawConfig() {
        }

        public bool Matches(Uri uri) => uri.Host == "pastebin.com" && uri.AbsolutePath.Length > 1;

        public IEnumerable<Uri>? ExtractUris(uint objectId, string info) {
            var matches = Matcher.Matches(info);
            return matches.Count == 0
                ? null
                : from Match match in matches select match.Groups[1].Value into id select new Uri($"https://pastebin.com/raw/{id}");
        }

        public async Task<ISearchInfoSection?> ExtractInfo(HttpResponseMessage response) {
            if (response.Content.Headers.ContentType?.MediaType != "text/plain") {
                return null;
            }

            var id = response.RequestMessage!.RequestUri!.AbsolutePath.Split('/').LastOrDefault();

            var info = await response.Content.ReadAsStringAsync();

            return new TextSection(this, $"Pastebin ({id})", response.RequestMessage.RequestUri, info);
        }
    }
}
