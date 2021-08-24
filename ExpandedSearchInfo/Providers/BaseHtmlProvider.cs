using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using ExpandedSearchInfo.Configs;
using ExpandedSearchInfo.Sections;

namespace ExpandedSearchInfo.Providers {
    public abstract class BaseHtmlProvider : IProvider {
        private IBrowsingContext Context { get; } = BrowsingContext.New();

        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract BaseConfig Config { get; }

        public abstract bool ExtractsUris { get; }

        public abstract void DrawConfig();

        public abstract bool Matches(Uri uri);

        public abstract IEnumerable<Uri>? ExtractUris(uint objectId, string info);

        public abstract Task<ISearchInfoSection?> ExtractInfo(HttpResponseMessage response);

        protected async Task<IHtmlDocument> DownloadDocument(HttpResponseMessage response) {
            var html = await response.Content.ReadAsStringAsync();
            var parser = this.Context.GetService<IHtmlParser>();
            return await parser.ParseDocumentAsync(html);
        }
    }
}
