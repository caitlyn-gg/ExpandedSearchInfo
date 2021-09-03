using System;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace ExpandedSearchInfo {
    public class PluginUi : IDisposable {
        private Plugin Plugin { get; }

        private bool _configVisible;

        internal bool ConfigVisible {
            get => this._configVisible;
            set => this._configVisible = value;
        }

        internal PluginUi(Plugin plugin) {
            this.Plugin = plugin;

            this.Plugin.Interface.UiBuilder.Draw += this.Draw;
            this.Plugin.Interface.UiBuilder.OpenConfigUi += this.OnOpenConfigUi;
        }

        private void OnOpenConfigUi() {
            this.ConfigVisible = true;
        }

        public void Dispose() {
            this.Plugin.Interface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
            this.Plugin.Interface.UiBuilder.Draw -= this.Draw;
        }

        private static bool IconButton(FontAwesomeIcon icon, string? id = null) {
            ImGui.PushFont(UiBuilder.IconFont);

            var text = icon.ToIconString();
            if (id != null) {
                text += $"##{id}";
            }

            var result = ImGui.Button(text);

            ImGui.PopFont();

            return result;
        }

        private void Draw() {
            this.DrawConfig();
            this.DrawExpandedSearchInfo();
        }

        private void DrawConfig() {
            if (!this.ConfigVisible) {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(500, -1), ImGuiCond.FirstUseEver);

            if (!ImGui.Begin($"{this.Plugin.Name} settings", ref this._configVisible)) {
                return;
            }

            ImGui.PushTextWrapPos();

            if (ImGui.Button("Clear cache")) {
                this.Plugin.Repository.SearchInfos.Clear();
            }

            ImGui.SameLine();

            var cached = this.Plugin.Repository.SearchInfos.Count;
            var playersString = cached switch {
                1 => "One player",
                _ => $"{cached} players",
            };
            ImGui.TextUnformatted($"{playersString} in the cache");

            ImGui.Spacing();

            ImGui.TextUnformatted("Expanded Search Info downloads information contained in search infos once and caches it for later retrieval. If you want to clear this cache, click the button above. You can also clear individual players from the cache with the button in their expanded info.");

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Providers", ImGuiTreeNodeFlags.DefaultOpen)) {
                if (!ImGui.BeginTabBar("ESI tabs")) {
                    return;
                }

                foreach (var provider in this.Plugin.Repository.AllProviders) {
                    if (!ImGui.BeginTabItem($"{provider.Name}##esi-provider")) {
                        continue;
                    }

                    ImGui.Columns(2);

                    ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() / 3);

                    ImGui.TextUnformatted(provider.Description);

                    ImGui.NextColumn();

                    var enabled = provider.Config.Enabled;
                    if (ImGui.Checkbox($"Enabled##{provider.Name}", ref enabled)) {
                        provider.Config.Enabled = enabled;
                        this.Plugin.Config.Save();
                    }

                    var defaultOpen = provider.Config.DefaultExpanded;
                    if (ImGui.Checkbox($"Open by default##{provider.Name}", ref defaultOpen)) {
                        provider.Config.DefaultExpanded = defaultOpen;
                        this.Plugin.Config.Save();
                    }

                    provider.DrawConfig();

                    ImGui.Columns(1);

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.PopTextWrapPos();

            ImGui.End();
        }

        private unsafe void DrawExpandedSearchInfo() {
            // check if the examine window is open
            var addonPtr = this.Plugin.GameGui.GetAddonByName("CharacterInspect", 1);
            if (addonPtr == IntPtr.Zero) {
                return;
            }

            var addon = (AtkUnitBase*) addonPtr;
            if (!addon->IsVisible) {
                return;
            }

            // get examine window info
            var rootNode = addon->RootNode;
            if (rootNode == null) {
                return;
            }

            var width = rootNode->Width * addon->Scale;
            var height = rootNode->Height * addon->Scale;
            var x = addon->X;
            var y = addon->Y;

            // check the last actor id recorded (should be who the examine window is showing)
            var actorId = this.Plugin.Repository.LastObjectId;
            if (actorId == 0 || !this.Plugin.Repository.SearchInfos.TryGetValue(actorId, out var expanded)) {
                return;
            }

            // set window size
            ImGui.SetNextWindowSizeConstraints(
                new Vector2(0, 0),
                new Vector2(ImGui.GetIO().DisplaySize.X / 4, height)
            );
            ImGui.SetNextWindowSize(new Vector2(-1, -1));

            if (!ImGui.Begin(this.Plugin.Name, ImGuiWindowFlags.NoTitleBar)) {
                ImGui.End();
                return;
            }

            ImGui.PushTextWrapPos(ImGui.GetIO().DisplaySize.X / 4 - 24);

            // show a section for each extracted section
            for (var i = 0; i < expanded.Sections.Count; i++) {
                var section = expanded.Sections[i];

                var flags = section.Provider.Config.DefaultExpanded switch {
                    true => ImGuiTreeNodeFlags.DefaultOpen,
                    false => ImGuiTreeNodeFlags.None,
                };
                if (!ImGui.CollapsingHeader($"{section.Name}##{i}", flags)) {
                    continue;
                }

                ImGui.TreePush();

                if (IconButton(FontAwesomeIcon.ExternalLinkAlt, $"open-{i}")) {
                    Process.Start(new ProcessStartInfo {
                        FileName = section.Uri.ToString(),
                        UseShellExecute = true,
                    });
                }

                if (ImGui.IsItemHovered()) {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted("Open in browser.");
                    ImGui.EndTooltip();
                }

                ImGui.SameLine();

                if (IconButton(FontAwesomeIcon.Redo, $"refresh-{i}")) {
                    this.Plugin.Repository.SearchInfos.TryRemove(actorId, out _);
                }

                if (ImGui.IsItemHovered()) {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted("Clear the cache. Re-examine this character to redownload information.");
                    ImGui.EndTooltip();
                }

                section.Draw();

                ImGui.TreePop();
            }

            ImGui.PopTextWrapPos();

            // determine whether to show on the left or right of the examine window based on space available
            var display = ImGui.GetIO().DisplaySize;
            var actualWidth = ImGui.GetWindowWidth();

            var xPos = x + width + actualWidth > display.X
                ? x - actualWidth
                : x + width;
            ImGui.SetWindowPos(ImGuiHelpers.MainViewport.Pos + new Vector2(xPos, y));

            ImGui.End();
        }
    }
}
