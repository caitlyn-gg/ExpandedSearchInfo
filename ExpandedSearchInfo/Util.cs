using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dalamud.Game.Text.SeStringHandling;
using ImGuiNET;

namespace ExpandedSearchInfo {
    internal static class Util {
        private static readonly Regex BbCodeTag = new(@"\[/?\w+(?:=.+?)?\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static unsafe SeString ReadRawSeString(IntPtr data) {
            var bytes = new List<byte>();

            var ptr = (byte*) data;
            while (*ptr != 0) {
                bytes.Add(*ptr);
                ptr += 1;
            }

            return SeString.Parse(bytes.ToArray());
        }

        internal static string StripBbCode(this string input) => BbCodeTag.Replace(input, "");

        internal static void DrawLines(string input) {
            // FIXME: this is a workaround for imgui breaking on extremely long strings
            foreach (var line in input.Split(new[] {"\n", "\r", "\r\n"}, StringSplitOptions.None)) {
                ImGui.TextUnformatted(line);
            }
        }
    }
}
