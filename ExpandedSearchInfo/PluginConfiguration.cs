using System;
using Dalamud.Configuration;
using ExpandedSearchInfo.Configs;

namespace ExpandedSearchInfo {
    [Serializable]
    public class PluginConfiguration : IPluginConfiguration {
        private Plugin Plugin { get; set; } = null!;

        public int Version { get; set; } = 1;

        public ProviderConfigs Configs { get; set; } = new();

        internal void Initialise(Plugin plugin) {
            this.Plugin = plugin;
        }

        internal void Save() {
            this.Plugin.Interface.SavePluginConfig(this);
        }
    }

    [Serializable]
    public class ProviderConfigs {
        public CarrdConfig Carrd { get; set; } = new();
        public FListConfig FList { get; set; } = new();
        public PastebinConfig Pastebin { get; set; } = new();
        public PlainTextConfig PlainText { get; set; } = new();
        public RefsheetConfig Refsheet { get; set; } = new();
    }
}
