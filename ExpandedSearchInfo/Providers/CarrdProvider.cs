using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using ExpandedSearchInfo.Sections;

namespace ExpandedSearchInfo.Providers {
    public class CarrdProvider : BaseHtmlProvider {
        public override bool ExtractsUris => false;

        public override bool Matches(Uri uri) => uri.Host.EndsWith(".carrd.co") || uri.Host.EndsWith(".crd.co");

        public override IEnumerable<Uri>? ExtractUris(int actorId, string info) => null;

        public override async Task<ISearchInfoSection?> ExtractInfo(HttpResponseMessage response) {
            var document = await this.DownloadDocument(response);

            var text = string.Empty;

            IElement? lastList = null;
            var listNum = 1;

            foreach (var element in document.QuerySelectorAll("p, [id ^= 'text']")) {
                // check if this element is in an li
                var inLi = element.ParentElement?.TagName == "LI";
                // if the first element in a li, we need to prefix it
                if (inLi && element.PreviousSibling == null) {
                    // check if this element is in the same list as the last list element we checked
                    if (element.ParentElement != lastList) {
                        // if not, update the last list and reset the counter
                        lastList = element.ParentElement;
                        listNum = 1;
                    }

                    // check if this list is an ol or ul
                    var isOl = element.ParentElement?.ParentElement?.TagName == "OL";
                    if (isOl) {
                        // use the list number for ol
                        text += $"{listNum++}. ";
                    } else {
                        // use a dash for ul
                        text += "- ";
                    }
                }

                // add the text from each child node
                foreach (var node in element.ChildNodes) {
                    text += node.Text();
                    // add an extra newline if the node is a br
                    if (node is IElement {TagName: "BR"}) {
                        text += '\n';
                    }
                }

                // add a newline after every element
                text += '\n';
            }

            return new TextSection(
                $"{document.Title} (carrd.co)",
                response.RequestMessage.RequestUri,
                text
            );
        }
    }
}
