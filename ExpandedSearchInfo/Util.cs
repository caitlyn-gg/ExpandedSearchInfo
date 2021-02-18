using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using ImGuiNET;

namespace ExpandedSearchInfo {
    internal static class Util {
        private static readonly Regex BbCodeTag = new(@"\[/?\w+(?:=.+?)?\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static string ReadRawString(IntPtr data) {
            var bytes = new List<byte>();

            for (var i = 0;; i++) {
                var b = Marshal.ReadByte(data, i);
                if (b == 0) {
                    break;
                }

                bytes.Add(b);
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        internal static string StripBbCode(string input) => BbCodeTag.Replace(input, "");

        internal static void DrawLines(string input) {
            // FIXME: this is a workaround for imgui breaking on extremely long strings
            foreach (var line in input.Split(new[] {"\n", "\r", "\r\n"}, StringSplitOptions.None)) {
                ImGui.TextUnformatted(line);
            }
        }
    }
}
