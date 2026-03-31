using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using monkeydroid.Models;
using monkeydroid.Services;

namespace monkeydroid.ViewModels;

public partial class PlaylistsViewModel : ViewModelBase
{
    public string Title => "Playlists";

    [ObservableProperty] private string _timestamp = "";

    public ObservableCollection<PlaylistInfo> Playlists { get; } = new();

    private CancellationTokenSource? _loadCts;

    public void LoadFromCache()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;

        Playlists.Clear();
        foreach (var p in server.Playlists.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
            Playlists.Add(p);

        UpdateTimestamp(server);
    }

    private void UpdateTimestamp(Models.Server server)
    {
        var ts = server.PlaylistsTimestamp?.ToString("g");
        Timestamp = ts is not null ? $"{server.Name}: {ts}" : server.Name;
    }

    [RelayCommand]
    private async Task LoadList()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;

        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();

        Playlists.Clear();
        Timestamp = server.Name;
        server.Playlists.Clear();

        try
        {
            await BackgroundListLoader.LoadPlaylistsAsync(server, info =>
            {
                Playlists.Add(info);
                server.Playlists.Add(info);
            }, _loadCts.Token);

            server.PlaylistsTimestamp = DateTime.Now;
            UpdateTimestamp(server);
            DataStore.Instance.Save();
        }
        catch (OperationCanceledException) { }
    }

    [RelayCommand]
    private async Task SelectPlaylist(PlaylistInfo playlist)
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;
        await CommsService.SendCommand(server, "--playlist", playlist.Name);
    }

    [RelayCommand]
    private async Task AddFx()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;
        await CommsService.SendCommand(server, "--next", "fx");
    }

    [RelayCommand]
    private async Task NextViz()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;
        await CommsService.SendCommand(server, "--next");
    }
}
