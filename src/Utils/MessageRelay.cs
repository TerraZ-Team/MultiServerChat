using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MultiServerChat.Configuration;
using Rests;
using TShockAPI;

namespace MultiServerChat.Utils;

internal sealed class MessageRelay
{
    private const string RelayEndpoint = "/jl";
    private static readonly HttpClient Client = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    public void SendChatMessage(TSPlayer player, string formattedText) =>
        _ = SendMessageToAllAsync(CreateChatMessage(player, formattedText));

    public void SendJoinMessage(TSPlayer player) =>
        _ = SendMessageToAllAsync(CreateJoinLeaveMessage(Config.Settings.JoinFormat, player.Name));

    public void SendLeaveMessage(TSPlayer player) =>
        _ = SendMessageToAllAsync(CreateJoinLeaveMessage(Config.Settings.LeaveFormat, player.Name));

    public void ReceiveMessage(RestRequestArgs args)
    {
        try
        {
            var message = ReadIncomingMessage(args);
            if (message is null || !ShouldDisplay(message))
            {
                return;
            }

            TShock.Utils.Broadcast(message.Text, message.Red, message.Green, message.Blue);
        }
        catch (Exception ex)
        {
            TShock.Log.Warn($"MultiServerChat rejected an invalid remote payload: {ex.Message}");
        }
    }

    private async Task SendMessageToAllAsync(Message message)
    {
        var settings = Config.Settings;
        var payload = message.ToString();
        var token = Uri.EscapeDataString(settings.Token);

        foreach (var url in settings.RestURLs)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var endpoint = url.TrimEnd('/');
            var uri = $"{endpoint}{RelayEndpoint}?token={token}";

            try
            {
                using var content = new StringContent(payload, Encoding.UTF8, "application/json");
                using var response = await Client.PostAsync(uri, content).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                TShock.Log.Error($"Failed to relay MultiServerChat message to '{endpoint}': {ex.Message}");
            }
        }
    }

    private static Message? ReadIncomingMessage(RestRequestArgs args) =>
        ReadMessageFromBody(args) ?? ReadMessageFromQuery(args.Parameters["message"]);

    private static Message? ReadMessageFromBody(RestRequestArgs args)
    {
        var request = args.GetType().GetProperty("Request")?.GetValue(args);
        if (request is null)
        {
            return null;
        }

        var bodyProperty = request.GetType().GetProperty("Body");
        if (bodyProperty?.GetValue(request) is not Stream { CanRead: true } body)
        {
            return null;
        }

        if (body.CanSeek)
        {
            body.Position = 0;
        }

        using var reader = new StreamReader(body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var payload = reader.ReadToEnd();
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        return Message.FromJson(payload);
    }

    private static Message? ReadMessageFromQuery(string? encodedMessage)
    {
        if (string.IsNullOrWhiteSpace(encodedMessage))
        {
            return null;
        }

        var decoded = Uri.UnescapeDataString(encodedMessage);
        var bytes = Convert.FromBase64String(decoded);
        var payload = Encoding.UTF8.GetString(bytes);
        return Message.FromJson(payload);
    }

    private static bool ShouldDisplay(Message message) =>
        message.Kind switch
        {
            MessageKind.JoinLeave => Config.Settings.DisplayJoinLeave,
            _ => Config.Settings.DisplayChat
        };

    private static Message CreateChatMessage(TSPlayer player, string formattedText) =>
        CreateMessage(
            string.Format(
                Config.Settings.ChatFormat,
                TShock.Config.Settings.ServerName,
                formattedText),
            MessageKind.Chat,
            player.Group?.R ?? byte.MaxValue,
            player.Group?.G ?? byte.MaxValue,
            player.Group?.B ?? byte.MaxValue);

    private static Message CreateJoinLeaveMessage(string format, string playerName) =>
        CreateMessage(
            string.Format(format, TShock.Config.Settings.ServerName, playerName),
            MessageKind.JoinLeave,
            Color.Yellow.R,
            Color.Yellow.G,
            Color.Yellow.B);

    private static Message CreateMessage(string text, MessageKind kind, byte red, byte green, byte blue) =>
        new()
        {
            Text = text,
            Kind = kind,
            Red = red,
            Green = green,
            Blue = blue
        };
}
