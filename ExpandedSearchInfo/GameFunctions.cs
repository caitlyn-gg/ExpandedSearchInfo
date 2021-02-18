using System;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using Dalamud.Plugin;

namespace ExpandedSearchInfo {
    public class GameFunctions : IDisposable {
        private Plugin Plugin { get; }

        private delegate byte SearchInfoDownloadedDelegate(IntPtr data, IntPtr a2, IntPtr searchInfoPtr, IntPtr a4);

        private readonly Hook<SearchInfoDownloadedDelegate>? _searchInfoDownloadedHook;

        internal delegate void ReceiveSearchInfoEventDelegate(int actorId, string info);

        internal event ReceiveSearchInfoEventDelegate? ReceiveSearchInfo;

        internal GameFunctions(Plugin plugin) {
            this.Plugin = plugin;

            var sidPtr = this.Plugin.Interface.TargetModuleScanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 56 48 83 EC 20 49 8B E8 8B DA");
            this._searchInfoDownloadedHook = new Hook<SearchInfoDownloadedDelegate>(sidPtr, new SearchInfoDownloadedDelegate(this.SearchInfoDownloaded));
            this._searchInfoDownloadedHook.Enable();
        }

        public void Dispose() {
            this._searchInfoDownloadedHook?.Dispose();
        }

        private byte SearchInfoDownloaded(IntPtr data, IntPtr a2, IntPtr searchInfoPtr, IntPtr a4) {
            var result = this._searchInfoDownloadedHook!.Original(data, a2, searchInfoPtr, a4);

            try {
                var actorId = Marshal.ReadInt32(data + 48);

                // var searchInfoPtr = data + 90;
                var searchInfo = Util.ReadRawString(searchInfoPtr);

                this.ReceiveSearchInfo?.Invoke(actorId, searchInfo);
            } catch (Exception ex) {
                PluginLog.LogError($"Error in SearchInfoDownloaded hook\n{ex}");
            }

            return result;
        }
    }
}
