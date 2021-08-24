using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace ExpandedSearchInfo {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : IDalamudPlugin {
        public string Name => "Expanded Search Info";

        [PluginService]
        internal DalamudPluginInterface Interface { get; init; } = null!;

        [PluginService]
        internal CommandManager CommandManager { get; init; } = null!;

        [PluginService]
        internal GameGui GameGui { get; init; } = null!;

        [PluginService]
        internal ObjectTable ObjectTable { get; init; } = null!;

        [PluginService]
        internal SeStringManager SeStringManager { get; init; } = null!;

        [PluginService]
        internal SigScanner SigScanner { get; init; } = null!;

        internal PluginConfiguration Config { get; }
        internal GameFunctions Functions { get; }
        internal SearchInfoRepository Repository { get; }
        private PluginUi Ui { get; }

        public Plugin() {
            this.Config = (PluginConfiguration?) this.Interface.GetPluginConfig() ?? new PluginConfiguration();
            this.Config.Initialise(this);

            this.Functions = new GameFunctions(this);
            this.Repository = new SearchInfoRepository(this);
            this.Ui = new PluginUi(this);

            this.CommandManager.AddHandler("/esi", new CommandInfo(this.OnCommand) {
                HelpMessage = "Toggles Expanded Search Info's configuration window",
            });
        }

        public void Dispose() {
            this.CommandManager.RemoveHandler("/esi");
            this.Ui.Dispose();
            this.Repository.Dispose();
            this.Functions.Dispose();
        }

        private void OnCommand(string command, string arguments) {
            this.Ui.ConfigVisible = !this.Ui.ConfigVisible;
        }
    }
}
