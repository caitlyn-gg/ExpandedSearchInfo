﻿using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;

namespace ExpandedSearchInfo {
    public class PluginUi : IDisposable {
        private Plugin Plugin { get; }

        internal PluginUi(Plugin plugin) {
            this.Plugin = plugin;

            this.Plugin.Interface.UiBuilder.OnBuildUi += this.Draw;
        }

        public void Dispose() {
            this.Plugin.Interface.UiBuilder.OnBuildUi -= this.Draw;
        }

        private void Draw() {
            // check if the examine window is open
            var addon = this.Plugin.Interface.Framework.Gui.GetAddonByName("CharacterInspect", 1);
            if (addon == null || !addon.Visible) {
                return;
            }

            // get examine window info
            float width;
            float height;
            short x;
            short y;

            try {
                width = addon.Width;
                height = addon.Height;
                x = addon.X;
                y = addon.Y;
            } catch (Exception) {
                return;
            }

            // check the last actor id recorded (should be who the examine window is showing)
            var actorId = this.Plugin.Repository.LastActorId;
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
                return;
            }

            ImGui.PushTextWrapPos(ImGui.GetIO().DisplaySize.X / 4 - 24);

            // show a section for each extracted section
            for (var i = 0; i < expanded.Sections.Count; i++) {
                var section = expanded.Sections[i];

                if (!ImGui.CollapsingHeader($"{section.Name}##{i}", ImGuiTreeNodeFlags.DefaultOpen)) {
                    continue;
                }

                ImGui.TreePush();

                if (ImGui.Button($"Open in browser##{i}")) {
                    Process.Start(section.Uri.ToString());
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
            ImGui.SetWindowPos(new Vector2(xPos, y));

            ImGui.End();
        }
    }
}
