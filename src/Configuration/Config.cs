using System;
using System.Collections.Generic;
using TShockAPI.Configuration;

namespace MultiServerChat.Configuration;

public static class Config
{
    private static readonly ConfigFile<ConfigSettings> File = new();
    private static string _path = string.Empty;

    public static ConfigSettings Settings => File.Settings;

    public static void Read(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = path;
        Read();
    }

    public static void Read()
    {
        if (string.IsNullOrWhiteSpace(_path))
        {
            throw new InvalidOperationException("Config path is not initialized.");
        }

        File.Read(_path, out var write);
        if (write)
        {
            File.Write(_path);
        }
    }
}

public sealed class ConfigSettings
{
    public List<string> RestURLs { get; set; } = [];
    public string Token { get; set; } = "abcdef";
    public string ChatFormat { get; set; } = "[{0}] {1}";
    public string JoinFormat { get; set; } = "[{0}] {1} has joined.";
    public string LeaveFormat { get; set; } = "[{0}] {1} has left.";
    public bool SendChat { get; set; } = true;
    public bool SendJoinLeave { get; set; } = true;
    public bool DisplayChat { get; set; } = true;
    public bool DisplayJoinLeave { get; set; } = true;
}
