using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;

namespace ExpandedSearchInfo;

public class GameFunctions : IDisposable {
    private Plugin Plugin { get; }

    private readonly Hook<AgentInspect.Delegates.ReceiveSearchComment>? _receiveSearchCommentHook;

    internal delegate void ReceiveSearchInfoEventDelegate(uint objectId, SeString info);

    internal event ReceiveSearchInfoEventDelegate? ReceiveSearchInfo;

    internal unsafe GameFunctions(Plugin plugin) {
        this.Plugin = plugin;

        this._receiveSearchCommentHook =
            this.Plugin.GameInteropProvider.HookFromAddress<AgentInspect.Delegates.ReceiveSearchComment>(
            AgentInspect.MemberFunctionPointers.ReceiveSearchComment,
            this.ReceiveSearchComment);

        this._receiveSearchCommentHook.Enable();
    }

    public void Dispose() {
        this._receiveSearchCommentHook?.Dispose();
    }

    private unsafe void ReceiveSearchComment(AgentInspect* _this, uint entityId, byte* searchComment) {
        this._receiveSearchCommentHook!.Original(_this, entityId, searchComment);

        try {
            var searchInfo = Util.ReadRawSeString((nint) searchComment);

            this.ReceiveSearchInfo?.Invoke(entityId, searchInfo);
        } catch (Exception ex) {
            Plugin.Log.Error(ex, "Error in ReceiveSearchComment hook");
        }
    }
}
