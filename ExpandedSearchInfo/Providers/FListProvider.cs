using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using ExpandedSearchInfo.Configs;
using ExpandedSearchInfo.Sections;

namespace ExpandedSearchInfo.Providers {
    public class FListProvider : BaseHtmlProvider {
        private Plugin Plugin { get; }

        public override string Name => "F-List (18+)";

        public override string Description => "This provider provides information for F-List URLs. It also searches for F-List profiles matching the character's name if /c/ is in their search info.";

        public override BaseConfig Config => this.Plugin.Config.Configs.FList;

        public override bool ExtractsUris => true;

        internal FListProvider(Plugin plugin) {
            this.Plugin = plugin;
        }

        public override void DrawConfig() {
        }

        public override bool Matches(Uri uri) => uri.Host is "www.f-list.net" or "f-list.net" && uri.AbsolutePath.StartsWith("/c/");

        public override IEnumerable<Uri>? ExtractUris(uint objectId, string info) {
            if (!info.ToLowerInvariant().Contains("c/")) {
                return null;
            }

            var obj = this.Plugin.ObjectTable.FirstOrDefault(obj => obj.ObjectId == objectId);
            if (obj == null) {
                return null;
            }

            var safeName = obj.Name.ToString().Replace("'", "");

            return new[] {
                new Uri($"https://www.f-list.net/c/{Uri.EscapeUriString(safeName)}"),
            };
        }

        public override async Task<ISearchInfoSection?> ExtractInfo(HttpResponseMessage response) {
            var document = await this.DownloadDocument(response);

            var error = document.QuerySelector("#DisplayedMessage");
            if (error != null) {
                var errorText = error.Text();

                if (errorText.Contains("No such character exists")) {
                    return null;
                }

                if (errorText.Contains("has been banned")) {
                    return null;
                }
            }

            var stats = new List<Tuple<string, string>>();
            var statBox = document.QuerySelector(".statbox");
            if (statBox != null) {
                foreach (var stat in statBox.Children) {
                    if (!stat.Matches(".taglabel")) {
                        continue;
                    }

                    var name = stat.Text().Trim().Trim(' ', '\r', '\n', '\t', ':');
                    var value = stat.NextSibling.Text().Trim(' ', '\r', '\n', '\t', ':');
                    stats.Add(new Tuple<string, string>(name, value));
                }
            }

            var info = string.Empty;
            var formatted = document.QuerySelector("#tabs-1 > .FormattedBlock");
            if (formatted != null) {
                foreach (var child in formatted.ChildNodes) {
                    info += child.Text();
                    if (child is IElement childElem && childElem.TagName != "BR") {
                        info += "\n";
                    }
                }
            }

            // remove bbcode and turn special characters into normal ascii
            info = info.StripBbCode().Normalize(NormalizationForm.FormKD);

            var fave = KinkSection(document, "Character_FetishlistFave");
            var yes = KinkSection(document, "Character_FetishlistYes");
            var maybe = KinkSection(document, "Character_FetishlistMaybe");
            var no = KinkSection(document, "Character_FetishlistNo");

            var charName = document.Title.Split('-')[2].Trim();
            return new FListSection(
                this,
                $"{charName} (F-List)",
                response.RequestMessage!.RequestUri!,
                info,
                stats,
                fave,
                yes,
                maybe,
                no
            );
        }

        private static List<Tuple<string, string>> KinkSection(IParentNode document, string id) {
            var kinks = new List<Tuple<string, string>>();
            var kinkElems = document.QuerySelectorAll($"#{id} > a");
            foreach (var kink in kinkElems) {
                var name = kink.Text().Trim();
                var value = kink.Attributes.GetNamedItem("rel")?.Value ?? "";
                kinks.Add(new Tuple<string, string>(name, value));
            }

            return kinks;
        }
    }
}
