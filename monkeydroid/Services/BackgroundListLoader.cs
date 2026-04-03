using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using monkeydroid.Models;

namespace monkeydroid.Services;

public static class BackgroundListLoader
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public static bool IsDownloading { get; private set; }
    public static event Action<bool>? IsDownloadingChanged;

    public static async Task LoadVisualizersAsync(
        Server server, Action<VisualizerInfo> onItem, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        IsDownloading = true;
        IsDownloadingChanged?.Invoke(true);
        CommsService.PushBusy();
        try
        {
            var names = await FetchFileList(server, "viz", ct);
            if (names is null) return;

            foreach (var name in names)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(250, ct);

                if (!await CommsService.SendCommand(server, "--md.detail", name))
                    continue;

                var response = CommsService.GetResponse();
                if (response.StartsWith("ERR", StringComparison.Ordinal)) return;
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
        finally
        {
            CommsService.PopBusy();
            IsDownloading = false;
            IsDownloadingChanged?.Invoke(false);
            _semaphore.Release();
        }
    }

    public static async Task LoadFxAsync(
        Server server, Action<FxInfo> onItem, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        IsDownloading = true;
        IsDownloadingChanged?.Invoke(true);
        CommsService.PushBusy();
        try
        {
            var names = await FetchFileList(server, "fx", ct);
            if (names is null) return;

            foreach (var name in names)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(250, ct);

                if (!await CommsService.SendCommand(server, "--md.detailfx", name))
                    continue;

                var response = CommsService.GetResponse();
                if (response.StartsWith("ERR", StringComparison.Ordinal)) return;
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
        finally
        {
            CommsService.PopBusy();
            IsDownloading = false;
            IsDownloadingChanged?.Invoke(false);
            _semaphore.Release();
        }
    }

    public static async Task LoadPlaylistsAsync(
        Server server, Action<PlaylistInfo> onItem, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        IsDownloading = true;
        IsDownloadingChanged?.Invoke(true);
        CommsService.PushBusy();
        try
        {
            var names = await FetchFileList(server, "playlists", ct);
            if (names is null) return;

            foreach (var name in names)
            {
                onItem(new PlaylistInfo { Name = name });
            }
        }
        finally
        {
            CommsService.PopBusy();
            IsDownloading = false;
            IsDownloadingChanged?.Invoke(false);
            _semaphore.Release();
        }
    }

    private static async Task<List<string>?> FetchFileList(
        Server server, string listType, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (!await CommsService.SendCommand(server, "--md.list", listType))
            return null;

        var response = CommsService.GetResponse();
        if (string.IsNullOrEmpty(response) || response.StartsWith("ERR", StringComparison.Ordinal))
            return null;

        return response
            .Split(CommsService.SeparatorCode, StringSplitOptions.RemoveEmptyEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
