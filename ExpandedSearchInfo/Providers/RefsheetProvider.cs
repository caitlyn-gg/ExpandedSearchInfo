using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ExpandedSearchInfo.Configs;
using ExpandedSearchInfo.Sections;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ExpandedSearchInfo.Providers {
    public class RefsheetProvider : BaseHtmlProvider {
        private const string JsonLineStart = "var props = ";

        private Plugin Plugin { get; }

        public override string Name => "Refsheet";

        public override string Description => "This provider provides information for refsheet.net and ref.st URLs.";

        public override BaseConfig Config => this.Plugin.Config.Configs.Refsheet;

        public override bool ExtractsUris => false;

        internal RefsheetProvider(Plugin plugin) {
            this.Plugin = plugin;
        }

        public override void DrawConfig() {
        }

        public override bool Matches(Uri uri) => uri.Host is "refsheet.net" or "ref.st";

        public override IEnumerable<Uri>? ExtractUris(uint objectId, string info) => null;

        public override async Task<ISearchInfoSection?> ExtractInfo(HttpResponseMessage response) {
            var document = await this.DownloadDocument(response);

            // refsheet provides all the content but... uses js to display it?
            // find the script containing the json and use it
            var script = document.QuerySelectorAll("script[type = 'text/javascript']").LastOrDefault();
            if (script == null) {
                return null;
            }

            var jsonLine = script.InnerHtml.Split('\n')
                .Select(line => line.Trim())
                .FirstOrDefault(line => line.StartsWith(JsonLineStart));
            if (jsonLine == null) {
                return null;
            }

            var json = jsonLine.Substring(JsonLineStart.Length, jsonLine.Length - JsonLineStart.Length - 1);
            var parsed = JsonConvert.DeserializeObject<RefsheetData>(json);
            if (parsed == null) {
                return null;
            }

            var character = parsed.EagerLoad.Character;

            // get character name
            var name = character.Name;

            // get all attributes
            var attributes = new List<Tuple<string, string>>();

            // handle built-in attributes first
            if (!string.IsNullOrWhiteSpace(character.Gender)) {
                attributes.Add(new Tuple<string, string>("Gender", character.Gender));
            }

            if (!string.IsNullOrWhiteSpace(character.Species)) {
                attributes.Add(new Tuple<string, string>("Species", character.Species));
            }

            if (!string.IsNullOrWhiteSpace(character.Height)) {
                attributes.Add(new Tuple<string, string>("Height", character.Height));
            }

            if (!string.IsNullOrWhiteSpace(character.Weight)) {
                attributes.Add(new Tuple<string, string>("Weight", character.Weight));
            }

            if (!string.IsNullOrWhiteSpace(character.BodyType)) {
                attributes.Add(new Tuple<string, string>("Body type", character.BodyType));
            }

            if (!string.IsNullOrWhiteSpace(character.Personality)) {
                attributes.Add(new Tuple<string, string>("Personality", character.Personality));
            }

            // then look for custom attributes
            foreach (var attr in character.CustomAttributes) {
                attributes.Add(new Tuple<string, string>(attr.Name, attr.Value));
            }

            // get important notes
            var notes = character.SpecialNotes;

            // get cards
            var cards = new List<Tuple<string, string>>();

            // get about card
            if (!string.IsNullOrWhiteSpace(character.Profile)) {
                cards.Add(new Tuple<string, string>($"About {character.Name}", character.Profile));
            }

            // get likes/dislikes cards
            if (!string.IsNullOrWhiteSpace(character.Likes)) {
                cards.Add(new Tuple<string, string>("Likes", character.Likes));
            }

            if (!string.IsNullOrWhiteSpace(character.Dislikes)) {
                cards.Add(new Tuple<string, string>("Dislikes", character.Dislikes));
            }

            return new RefsheetSection(
                this,
                $"{name} (Refsheet)",
                response.RequestMessage!.RequestUri!,
                attributes,
                notes,
                cards
            );
        }

        #pragma warning disable 8618
        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        private class RefsheetData {
            public RefsheetEagerLoad EagerLoad { get; set; }
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
        private class RefsheetEagerLoad {
            public RefsheetCharacter Character { get; set; }
        }

        [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        private class RefsheetCharacter {
            public string Name { get; set; }
            public string Profile { get; set; }
            public string Gender { get; set; }
            public string Species { get; set; }
            public string Height { get; set; }
            public string Weight { get; set; }
            public string BodyType { get; set; }
            public string Personality { get; set; }
            public string SpecialNotes { get; set; }
            public string Likes { get; set; }
            public string Dislikes { get; set; }
            public List<RefsheetCustomAttribute> CustomAttributes { get; set; }
        }

        [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        private class RefsheetCustomAttribute {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Id { get; set; }
        }
        #pragma warning restore 8618
    }
}
