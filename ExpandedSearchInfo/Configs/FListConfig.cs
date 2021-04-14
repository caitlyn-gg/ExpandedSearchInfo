using Newtonsoft.Json;

namespace ExpandedSearchInfo.Configs {
    public class FListConfig : BaseConfig {
        [JsonConstructor]
        public FListConfig() {
            this.Enabled = false;
        }
    }
}
