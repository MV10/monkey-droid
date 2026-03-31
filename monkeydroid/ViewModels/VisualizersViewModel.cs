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

public partial class VisualizersViewModel : ViewModelBase
{
    public string Title => "Visualizers";

    [ObservableProperty] private string _timestamp = "";

    public ObservableCollection<VisualizerInfo> Visualizers { get; } = new();

    private CancellationTokenSource? _loadCts;

    public void LoadFromCache()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;

        Visualizers.Clear();
        foreach (var v in server.Visualizers.OrderBy(v => v.Name, StringComparer.OrdinalIgnoreCase))
            Visualizers.Add(v);

        UpdateTimestamp(server);
    }

    private void UpdateTimestamp(Models.Server server)
    {
        var ts = server.VisualizersTimestamp?.ToString("g");
        Timestamp = ts is not null ? $"{server.Name}: {ts}" : server.Name;
    }

    [RelayCommand]
    private async Task LoadList()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;

        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();

        Visualizers.Clear();
        Timestamp = server.Name;
        server.Visualizers.Clear();

        try
        {
            await BackgroundListLoader.LoadVisualizersAsync(server, info =>
            {
                Visualizers.Add(info);
                server.Visualizers.Add(info);
            }, _loadCts.Token);

            server.VisualizersTimestamp = DateTime.Now;
            UpdateTimestamp(server);
            DataStore.Instance.Save();
        }
        catch (OperationCanceledException) { }
    }

    [RelayCommand]
    private async Task SelectVisualizer(VisualizerInfo viz)
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;
        await CommsService.SendCommand(server, "--load", viz.Name);
    }

    [RelayCommand]
    private async Task Reload()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;
        await CommsService.SendCommand(server, "--reload");
    }
}
