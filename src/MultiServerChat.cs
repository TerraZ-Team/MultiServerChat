using System;
using System.IO;
using MultiServerChat.Configuration;
using MultiServerChat.Handlers;
using MultiServerChat.Utils;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace MultiServerChat;

[ApiVersion(2, 1)]
public sealed class MultiServerChat : TerrariaPlugin
{
    private const string ConfigFileName = "multiserverchat.json";
    private readonly MessageRelay _messageRelay;
    private readonly CommandHandler _commandHandler;
    private readonly EventHooksHandler _eventHooksHandler;
    private readonly RestApiHandler _restApiHandler;

    public override string Author => "Zack Piispanen, now maintained and updated by Ryozuki";
    public override string Description => "Facilitate chat between servers.";
    public override string Name => "Multiserver Chat";
    public override Version Version => new(1, 0, 0, 6);

    public MultiServerChat(Main game) : base(game)
    {
        Order = 10;
        _messageRelay = new MessageRelay();
        _commandHandler = new CommandHandler(this);
        _eventHooksHandler = new EventHooksHandler(this, _messageRelay);
        _restApiHandler = new RestApiHandler(_messageRelay);
    }

    public override void Initialize()
    {
        var configPath = Path.Combine(TShock.SavePath, ConfigFileName);
        Config.Read(configPath);

        _commandHandler.Register();
        _eventHooksHandler.Register();
        _restApiHandler.Register();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _restApiHandler.Unregister();
            _eventHooksHandler.Unregister();
            _commandHandler.Unregister();
        }

        base.Dispose(disposing);
    }
}
