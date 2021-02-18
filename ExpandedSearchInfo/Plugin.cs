using Dalamud.Plugin;

namespace ExpandedSearchInfo {
    public class Plugin : IDalamudPlugin {
        public string Name => "Expanded Search Info";

        internal DalamudPluginInterface Interface { get; private set; } = null!;
        internal GameFunctions Functions { get; private set; } = null!;
        internal SearchInfoRepository Repository { get; private set; } = null!;
        private PluginUi Ui { get; set; } = null!;

        public void Initialize(DalamudPluginInterface pluginInterface) {
            this.Interface = pluginInterface;

            this.Functions = new GameFunctions(this);
            this.Repository = new SearchInfoRepository(this);
            this.Ui = new PluginUi(this);
        }

        public void Dispose() {
            this.Ui.Dispose();
            this.Repository.Dispose();
            this.Functions.Dispose();
        }
    }
}
