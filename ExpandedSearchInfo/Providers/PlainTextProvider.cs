using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ExpandedSearchInfo.Configs;
using ExpandedSearchInfo.Sections;

namespace ExpandedSearchInfo.Providers {
    public class PlainTextProvider : IProvider {
        private Plugin Plugin { get; }

        public string Name => "Plain text";

        public string Description => "This provider provides information for any URL that provides plain text.";

        public BaseConfig Config => this.Plugin.Config.Configs.PlainText;

        public bool ExtractsUris => false;

        internal PlainTextProvider(Plugin plugin) {
            this.Plugin = plugin;
        }

        public void DrawConfig() {
        }

        public bool Matches(Uri uri) => true;

        public IEnumerable<Uri>? ExtractUris(uint objectId, string info) => null;

        public async Task<ISearchInfoSection?> ExtractInfo(HttpResponseMessage response) {
            if (response.Content.Headers.ContentType?.MediaType != "text/plain") {
                return null;
            }

            var info = await response.Content.ReadAsStringAsync();

            var uri = response.RequestMessage!.RequestUri!;
            return new TextSection(this, $"Text##{uri}", uri, info);
        }

        public void ModifyRequest(HttpRequestMessage request) {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        }
    }
}
