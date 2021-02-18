using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ExpandedSearchInfo.Sections;

namespace ExpandedSearchInfo.Providers {
    public class PlainTextProvider : IProvider {
        public bool ExtractsUris => false;

        public bool Matches(Uri uri) => true;

        public IEnumerable<Uri>? ExtractUris(int actorId, string info) => null;

        public async Task<ISearchInfoSection?> ExtractInfo(HttpResponseMessage response) {
            if (response.Content.Headers.ContentType.MediaType != "text/plain") {
                return null;
            }

            var info = await response.Content.ReadAsStringAsync();

            var uri = response.RequestMessage.RequestUri;
            return new TextSection($"Text##{uri}", response.RequestMessage.RequestUri, info);
        }
    }
}
