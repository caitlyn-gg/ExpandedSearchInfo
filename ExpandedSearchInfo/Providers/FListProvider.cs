using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Dalamud.Plugin;
using ExpandedSearchInfo.Sections;

namespace ExpandedSearchInfo.Providers {
    public class FListProvider : BaseHtmlProvider {

        private Plugin Plugin { get; }

        public override bool ExtractsUris => true;

        internal FListProvider(Plugin plugin) {
            this.Plugin = plugin;
        }

        public override bool Matches(Uri uri) => (uri.Host == "www.f-list.net" || uri.Host == "f-list.net") && uri.AbsolutePath.StartsWith("/c/");

        public override IEnumerable<Uri>? ExtractUris(int actorId, string info) {
            if (!info.ToLowerInvariant().Contains("c/")) {
                return null;
            }

            var actor = this.Plugin.Interface.ClientState.Actors.FirstOrDefault(actor => actor.ActorId == actorId);
            if (actor == null) {
                return null;
            }

            var safeName = actor.Name.Replace("'", "");

            return new[] {
                new Uri($"https://www.f-list.net/c/{Uri.EscapeUriString(safeName)}"),
            };
        }

        public override async Task<ISearchInfoSection?> ExtractInfo(HttpResponseMessage response) {
            var document = await this.DownloadDocument(response);

            var error = document.QuerySelector("#DisplayedMessage");
            if (error != null) {
                if (error.Text().Contains("No such character exists")) {
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
            info = Util.StripBbCode(info).Normalize(NormalizationForm.FormKD);

            var fave = KinkSection(document, "Character_FetishlistFave");
            var yes = KinkSection(document, "Character_FetishlistYes");
            var maybe = KinkSection(document, "Character_FetishlistMaybe");
            var no = KinkSection(document, "Character_FetishlistNo");

            var charName = document.Title.Split('-')[2].Trim();
            return new FListSection(
                $"{charName} (F-List)",
                response.RequestMessage.RequestUri,
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
