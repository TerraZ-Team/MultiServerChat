using MultiServerChat.Configuration;
using MultiServerChat.Utils;
using TerrariaApi.Server;
using TShockAPI;

namespace MultiServerChat.Handlers;

internal sealed class CommandHandler(TerrariaPlugin plugin)
{
    private const string ReloadCommandName = "msc_reload";
    private Command? _reloadCommand;

    public void Register() => ServerApi.Hooks.GameInitialize.Register(plugin, OnInitialize);

    public void Unregister()
    {
        ServerApi.Hooks.GameInitialize.Deregister(plugin, OnInitialize);

        if (_reloadCommand is null)
        {
            return;
        }

        Commands.ChatCommands.Remove(_reloadCommand);
        _reloadCommand = null;
    }

    private void OnInitialize(EventArgs args)
    {
        if (_reloadCommand is not null)
        {
            return;
        }

        _reloadCommand = new Command(Permissions.Reload, ReloadCommand, ReloadCommandName)
        {
            HelpText = $"Usage: {TShock.Config.Settings.CommandSpecifier}{ReloadCommandName}"
        };

        Commands.ChatCommands.Add(_reloadCommand);
    }

    private static void ReloadCommand(CommandArgs args)
    {
        Config.Read();
        args.Player.SendSuccessMessage("MultiServerChat config reloaded.");
    }
}
