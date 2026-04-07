using MultiServerChat.Configuration;
using MultiServerChat.Utils;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace MultiServerChat.Handlers;

internal sealed class EventHooksHandler(TerrariaPlugin plugin, MessageRelay messageRelay)
{
    public void Register()
    {
        GeneralHooks.ReloadEvent += OnReload;
        PlayerHooks.PlayerChat += OnChat;
        ServerApi.Hooks.ServerJoin.Register(plugin, OnJoin, 10);
        ServerApi.Hooks.ServerLeave.Register(plugin, OnLeave, 10);
    }

    public void Unregister()
    {
        PlayerHooks.PlayerChat -= OnChat;
        GeneralHooks.ReloadEvent -= OnReload;
        ServerApi.Hooks.ServerJoin.Deregister(plugin, OnJoin);
        ServerApi.Hooks.ServerLeave.Deregister(plugin, OnLeave);
    }

    private static void OnReload(ReloadEventArgs args)
    {
        if (args.Player is not null && !args.Player.Group.HasPermission(Permissions.Reload))
        {
            return;
        }

        Config.Read();
    }

    private void OnChat(PlayerChatEventArgs args)
    {
        if (!Config.Settings.SendChat || args.Handled)
        {
            return;
        }

        messageRelay.SendChatMessage(args.Player, args.TShockFormattedText);
    }

    private void OnJoin(JoinEventArgs args)
    {
        if (!ShouldSendJoinLeave())
        {
            return;
        }

        var player = TShock.Players[args.Who];
        if (player is null || player.SilentJoinInProgress || !player.ReceivedInfo)
        {
            return;
        }

        messageRelay.SendJoinMessage(player);
    }

    private void OnLeave(LeaveEventArgs args)
    {
        if (!ShouldSendJoinLeave())
        {
            return;
        }

        var player = TShock.Players[args.Who];
        if (player is null || player.SilentKickInProgress || !player.ReceivedInfo)
        {
            return;
        }

        messageRelay.SendLeaveMessage(player);
    }

    private static bool ShouldSendJoinLeave() => Config.Settings.SendJoinLeave;
}
