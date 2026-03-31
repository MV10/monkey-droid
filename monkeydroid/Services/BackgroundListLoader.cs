using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using monkeydroid.Models;

namespace monkeydroid.Services;

public static class BackgroundListLoader
{
    public static async Task LoadVisualizersAsync(
        Server server, Action<VisualizerInfo> onItem, CancellationToken ct)
    {
        var names = await FetchFileList(server, "viz", ct);
        if (names is null) return;

        foreach (var name in names)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Delay(500, ct);

            if (!await CommsService.SendCommand(server, "--md.detail", name))
                continue;

            var response = CommsService.GetResponse();
            if (response.Length < 2) continue;

            var info = new VisualizerInfo
            {
                Name = name,
                Audio = response[0] == '1',
                Description = response[1..],
            };
            onItem(info);
        }
    }

    public static async Task LoadFxAsync(
        Server server, Action<FxInfo> onItem, CancellationToken ct)
    {
        var names = await FetchFileList(server, "fx", ct);
        if (names is null) return;

        foreach (var name in names)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Delay(500, ct);

            if (!await CommsService.SendCommand(server, "--md.detailfx", name))
                continue;

            var response = CommsService.GetResponse();
            if (response.Length < 2) continue;

            var info = new FxInfo
            {
                Name = name,
                Audio = response[0] == '1',
                Description = response[1..],
            };
            onItem(info);
        }
    }

    public static async Task LoadPlaylistsAsync(
        Server server, Action<PlaylistInfo> onItem, CancellationToken ct)
    {
        var names = await FetchFileList(server, "playlists", ct);
        if (names is null) return;

        foreach (var name in names)
        {
            onItem(new PlaylistInfo { Name = name });
        }
    }

    private static async Task<List<string>?> FetchFileList(
        Server server, string listType, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!await CommsService.SendCommand(server, "--md.list", listType))
            return null;

        var response = CommsService.GetResponse();
        if (string.IsNullOrEmpty(response))
            return null;

        return response
            .Split(CommsService.SeparatorCode, StringSplitOptions.RemoveEmptyEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
