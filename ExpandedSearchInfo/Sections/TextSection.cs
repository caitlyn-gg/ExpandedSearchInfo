using System;
using ExpandedSearchInfo.Providers;

namespace ExpandedSearchInfo.Sections {
    public class TextSection : ISearchInfoSection {
        public IProvider Provider { get; }
        public string Name { get; }
        public Uri Uri { get; }

        private string Info { get; }

        internal TextSection(IProvider provider, string name, Uri uri, string info) {
            this.Provider = provider;
            this.Name = name;
            this.Uri = uri;
            this.Info = info;
        }

        public void Draw() {
            Util.DrawLines(this.Info);
        }
    }
}
