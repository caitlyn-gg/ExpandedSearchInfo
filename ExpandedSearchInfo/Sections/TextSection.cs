using System;
using ImGuiNET;

namespace ExpandedSearchInfo.Sections {
    public class TextSection : ISearchInfoSection {
        private string Info { get; }
        public string Name { get; }
        public Uri Uri { get; }

        internal TextSection(string name, Uri uri, string info) {
            this.Name = name;
            this.Uri = uri;
            this.Info = info;
        }

        public void Draw() {
            Util.DrawLines(this.Info);
        }
    }
}
