using System.Text.Json;
using System.Text.Json.Serialization;

namespace MultiServerChat;

public enum MessageKind
{
    Chat,
    JoinLeave
}

public sealed class Message
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string Text { get; set; } = string.Empty;
    public MessageKind Kind { get; set; } = MessageKind.Chat;
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this, JsonOptions);

    public static Message FromJson(string json) =>
        JsonSerializer.Deserialize<Message>(json, JsonOptions)
        ?? throw new JsonException("Message payload is empty.");
}
