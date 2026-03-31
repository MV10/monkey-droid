using System.Threading.Tasks;
using CommandLineSwitchPipe;
using monkeydroid.Models;

namespace monkeydroid.Services;

public static class CommsService
{
    public static string SeparatorCode =>
        CommandLineSwitchServer.Options.Advanced.SeparatorControlCode;

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
        if (await CommandLineSwitchServer.TrySendArgs(args, server.Name, server.Port))
            return true;

        if (server.AlternatePort.HasValue)
            return await CommandLineSwitchServer.TrySendArgs(args, server.Name, server.AlternatePort.Value);

        return false;
    }

    public static string GetResponse() => CommandLineSwitchServer.QueryResponse ?? "";
}
