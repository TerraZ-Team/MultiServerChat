using System.Collections.Generic;
using System.Reflection;
using MultiServerChat.Utils;
using Rests;
using TShockAPI;

namespace MultiServerChat.Handlers;

internal sealed class RestApiHandler(MessageRelay messageRelay)
{
    private const string ChatEndpoint = "/msc";
    private const string RelayEndpoint = "/jl";
    private static readonly FieldInfo? CommandsField =
        typeof(Rest).GetField("commands", BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly List<RestCommand> _registeredCommands = [];

    public void Register()
    {
        Register(new SecureRestCommand(ChatEndpoint, HandleRequest, Permissions.Relay));
        Register(new SecureRestCommand(RelayEndpoint, HandleRequest, Permissions.Relay));
    }

    public void Unregister()
    {
        if (CommandsField?.GetValue(TShock.RestApi) is not List<RestCommand> commands)
        {
            TShock.Log.Warn("MultiServerChat could not unregister REST commands.");
            _registeredCommands.Clear();
            return;
        }

        foreach (var command in _registeredCommands)
        {
            commands.Remove(command);
        }

        _registeredCommands.Clear();
    }

    private void Register(RestCommand command)
    {
        TShock.RestApi.Register(command);
        _registeredCommands.Add(command);
    }

    private object HandleRequest(RestRequestArgs args)
    {
        messageRelay.ReceiveMessage(args);
        return new RestObject();
    }
}
