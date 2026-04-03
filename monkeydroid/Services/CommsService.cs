using System;
using System.Threading.Tasks;
using CommandLineSwitchPipe;
using monkeydroid.Models;

namespace monkeydroid.Services;

public static class CommsService
{
    private const int TimeoutMs = 1000;

    public static string SeparatorCode =>
        CommandLineSwitchServer.Options.Advanced.SeparatorControlCode;

    public static event Action<string>? ErrorReceived;
    public static bool SuppressErrorEvent { get; set; }

    private static int _busyCount;
    public static bool IsBusy => _busyCount > 0;
    public static event Action<bool>? BusyChanged;

    public static void PushBusy()
    {
        if (System.Threading.Interlocked.Increment(ref _busyCount) == 1)
            BusyChanged?.Invoke(true);
    }

    public static void PopBusy()
    {
        if (System.Threading.Interlocked.Decrement(ref _busyCount) == 0)
            BusyChanged?.Invoke(false);
    }

    public static async Task<bool> TryConnect(Server server)
    {
        PushBusy();
        try
        {
            return await FindRespondingPort(server) > 0;
        }
        finally { PopBusy(); }
    }

    public static async Task<bool> SendCommand(Server server, params string[] args)
    {
        PushBusy();
        try
        {
            var port = await FindRespondingPort(server);
            if (port == 0)
                return false;

            var sent = await WithTimeout(
                CommandLineSwitchServer.TrySendArgs(args, server.Name, port));

            if (sent)
            {
                var response = CommandLineSwitchServer.QueryResponse ?? "";
                if (!SuppressErrorEvent && response.StartsWith("ERR", StringComparison.Ordinal))
                    ErrorReceived?.Invoke(response);
            }

            return sent;
        }
        finally { PopBusy(); }
    }

    public static string GetResponse() => CommandLineSwitchServer.QueryResponse ?? "";

    private static async Task<int> FindRespondingPort(Server server)
    {
        if (!server.AlternatePort.HasValue)
            return await WithTimeout(
                CommandLineSwitchServer.TryConnect(server.Name, server.Port)) ? server.Port : 0;

        var primaryTask = CommandLineSwitchServer.TryConnect(server.Name, server.Port);
        var alternateTask = CommandLineSwitchServer.TryConnect(server.Name, server.AlternatePort.Value);
        var timeoutTask = Task.Delay(TimeoutMs);

        while (true)
        {
            var completed = await Task.WhenAny(primaryTask, alternateTask, timeoutTask);

            if (completed == timeoutTask)
                return 0;

            if (completed == primaryTask && primaryTask.Result)
                return server.Port;

            if (completed == alternateTask && alternateTask.Result)
                return server.AlternatePort.Value;

            // Both finished with false, or one finished false and other still running
            if (primaryTask.IsCompleted && alternateTask.IsCompleted)
                return 0;
        }
    }

    private static async Task<bool> WithTimeout(Task<bool> task)
    {
        if (await Task.WhenAny(task, Task.Delay(TimeoutMs)) == task)
            return task.Result;
        return false;
    }
}
