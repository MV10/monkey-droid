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

public partial class FxViewModel : ViewModelBase
{
    public string Title => "FX";

    [ObservableProperty] private string _timestamp = "";

    public ObservableCollection<FxInfo> FxList { get; } = new();

    private CancellationTokenSource? _loadCts;

    public void LoadFromCache()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;

        FxList.Clear();
        foreach (var f in server.Fx.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase))
            FxList.Add(f);

        UpdateTimestamp(server);
    }

    private void UpdateTimestamp(Models.Server server)
    {
        var ts = server.FxTimestamp?.ToString("g") ?? "";
        Timestamp = string.IsNullOrEmpty(ts) ? "" : $"{server.Name}: {ts}";
    }

    [RelayCommand]
    private async Task LoadList()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;

        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();

        FxList.Clear();
        Timestamp = "";
        server.Fx.Clear();

        try
        {
            await BackgroundListLoader.LoadFxAsync(server, info =>
            {
                FxList.Add(info);
                server.Fx.Add(info);
            }, _loadCts.Token);

            server.FxTimestamp = DateTime.Now;
            UpdateTimestamp(server);
            DataStore.Instance.Save();
        }
        catch (OperationCanceledException) { }
    }

    [RelayCommand]
    private async Task SelectFx(FxInfo fx)
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;
        await CommsService.SendCommand(server, "--fx", fx.Name);
    }
}
