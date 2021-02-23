using System;
using System.Collections.Generic;
using ExpandedSearchInfo.Providers;
using ImGuiNET;

namespace ExpandedSearchInfo.Sections {
    public class RefsheetSection : ISearchInfoSection {
        public IProvider Provider { get; }
        public string Name { get; }
        public Uri Uri { get; }

        private List<Tuple<string, string>> Attributes { get; }
        private string Notes { get; }
        private List<Tuple<string, string>> Cards { get; }

        internal RefsheetSection(IProvider provider, string name, Uri uri, List<Tuple<string, string>> attributes, string notes, List<Tuple<string, string>> cards) {
            this.Provider = provider;
            this.Name = name;
            this.Uri = uri;
            this.Attributes = attributes;
            this.Notes = notes;
            this.Cards = cards;
        }

        public void Draw() {
            if (ImGui.CollapsingHeader($"Attributes##{this.Name}")) {
                foreach (var (key, value) in this.Attributes) {
                    ImGui.TextUnformatted($"{key}: {value}");
                }
            }

            if (ImGui.CollapsingHeader($"Notes##{this.Name}")) {
                Util.DrawLines(this.Notes);
            }

            foreach (var (cardName, cardContent) in this.Cards) {
                if (!ImGui.CollapsingHeader($"{cardName}##{this.Name}")) {
                    continue;
                }

                Util.DrawLines(cardContent);
            }
        }
    }
}
