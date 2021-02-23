using Dalamud.Game.Command;
using Dalamud.Plugin;

namespace ExpandedSearchInfo {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Plugin : IDalamudPlugin {
        public string Name => "Expanded Search Info";

        internal PluginConfiguration Config { get; private set; } = null!;
        internal DalamudPluginInterface Interface { get; private set; } = null!;
        internal GameFunctions Functions { get; private set; } = null!;
        internal SearchInfoRepository Repository { get; private set; } = null!;
        private PluginUi Ui { get; set; } = null!;

        public void Initialize(DalamudPluginInterface pluginInterface) {
            this.Interface = pluginInterface;

            this.Config = (PluginConfiguration?) this.Interface.GetPluginConfig() ?? new PluginConfiguration();
            this.Config.Initialise(this);

            this.Functions = new GameFunctions(this);
            this.Repository = new SearchInfoRepository(this);
            this.Ui = new PluginUi(this);

            this.Interface.CommandManager.AddHandler("/esi", new CommandInfo(this.OnCommand) {
                HelpMessage = "Toggles Expanded Search Info's configuration window",
            });
        }

        public void Dispose() {
            this.Interface.CommandManager.RemoveHandler("/esi");
            this.Ui.Dispose();
            this.Repository.Dispose();
            this.Functions.Dispose();
        }

        private void OnCommand(string command, string arguments) {
            this.Ui.ConfigVisible = !this.Ui.ConfigVisible;
        }
    }
}
