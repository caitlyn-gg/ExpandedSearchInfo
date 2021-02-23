using System;
using System.Collections.Generic;
using System.Linq;
using ExpandedSearchInfo.Providers;
using ImGuiNET;

namespace ExpandedSearchInfo.Sections {
    public class FListSection : ISearchInfoSection {
        public IProvider Provider { get; }
        public string Name { get; }
        public Uri Uri { get; }

        private string Info { get; }
        private List<Tuple<string, string>> Stats { get; }
        private List<Tuple<string, string>> Fave { get; }
        private List<Tuple<string, string>> Yes { get; }
        private List<Tuple<string, string>> Maybe { get; }
        private List<Tuple<string, string>> No { get; }

        internal FListSection(IProvider provider, string name, Uri uri, string info, List<Tuple<string, string>> stats, List<Tuple<string, string>> fave, List<Tuple<string, string>> yes, List<Tuple<string, string>> maybe, List<Tuple<string, string>> no) {
            this.Provider = provider;
            this.Name = name;
            this.Uri = uri;

            this.Info = info;
            this.Stats = stats;
            this.Fave = fave;
            this.Yes = yes;
            this.Maybe = maybe;
            this.No = no;
        }

        public void Draw() {
            if (ImGui.CollapsingHeader($"Stats##{this.Name}")) {
                var stats = string.Join("\n", this.Stats.Select(entry => $"{entry.Item1}: {entry.Item2}"));
                ImGui.TextUnformatted(stats);
            }

            if (ImGui.CollapsingHeader($"Info##{this.Name}", ImGuiTreeNodeFlags.DefaultOpen)) {
                Util.DrawLines(this.Info);
            }

            this.DrawKinkSection("Fave", this.Fave);
            this.DrawKinkSection("Yes", this.Yes);
            this.DrawKinkSection("Maybe", this.Maybe);
            this.DrawKinkSection("No", this.No);
        }

        private void DrawKinkSection(string sectionName, IEnumerable<Tuple<string, string>> kinks) {
            if (!ImGui.CollapsingHeader($"{sectionName}##{this.Name}")) {
                return;
            }

            foreach (var (name, description) in kinks) {
                ImGui.TextUnformatted(name);
                if (!ImGui.IsItemHovered()) {
                    continue;
                }

                ImGui.BeginTooltip();

                ImGui.PushTextWrapPos(ImGui.GetIO().DisplaySize.X / 8);
                ImGui.TextUnformatted(description);
                ImGui.PopTextWrapPos();

                ImGui.EndTooltip();
            }
        }
    }
}
