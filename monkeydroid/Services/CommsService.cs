using System;
using System.Threading.Tasks;
using CommandLineSwitchPipe;
using monkeydroid.Models;

namespace monkeydroid.Services;

public static class CommsService
{
    public static string SeparatorCode =>
        CommandLineSwitchServer.Options.Advanced.SeparatorControlCode;

    public static event Action<string>? ErrorReceived;
    public static bool SuppressErrorEvent { get; set; }

    public static async Task<bool> TryConnect(Server server)
    {
        if (await CommandLineSwitchServer.TryConnect(server.Name, server.Port))
            return true;

        if (server.AlternatePort.HasValue)
            return await CommandLineSwitchServer.TryConnect(server.Name, server.AlternatePort.Value);

        return false;
    }

    public static async Task<bool> SendCommand(Server server, params string[] args)
    {
        bool sent;
        if (await CommandLineSwitchServer.TrySendArgs(args, server.Name, server.Port))
            sent = true;
        else if (server.AlternatePort.HasValue)
            sent = await CommandLineSwitchServer.TrySendArgs(args, server.Name, server.AlternatePort.Value);
        else
            sent = false;

        if (sent)
        {
            var response = CommandLineSwitchServer.QueryResponse ?? "";
            if (!SuppressErrorEvent && response.StartsWith("ERR", StringComparison.Ordinal))
                ErrorReceived?.Invoke(response);
        }

        return sent;
    }

    public static string GetResponse() => CommandLineSwitchServer.QueryResponse ?? "";
}
