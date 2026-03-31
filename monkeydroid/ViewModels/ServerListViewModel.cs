using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using monkeydroid.Models;
using monkeydroid.Services;

namespace monkeydroid.ViewModels;

public partial class ServerListViewModel : ViewModelBase
{
    public string Title => "Server List";

    [ObservableProperty] private string? _selectedServerName;

    public ObservableCollection<Server> Servers { get; } = new();

    public event Action? AddRequested;
    public event Action<Server>? EditRequested;
    public event Action<Server>? ServerTapped;
    public event Action<Server>? TestRequested;
    public event Action<Server>? DeleteRequested;
    public event Action<Server>? SelectRequested;

    public void Refresh()
    {
        Servers.Clear();
        foreach (var s in DataStore.Instance.Data.Servers.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase))
            Servers.Add(s);
        SelectedServerName = DataStore.Instance.SelectedServerName;
    }

    public void OnServerTapped(Server server)
    {
        ServerTapped?.Invoke(server);
    }

    public void RequestAdd() => AddRequested?.Invoke();

    public void RequestEdit(Server server) => EditRequested?.Invoke(server);

    public void RequestTest(Server server) => TestRequested?.Invoke(server);

    public void RequestDelete(Server server) => DeleteRequested?.Invoke(server);

    public void RequestSelect(Server server)
    {
        DataStore.Instance.SelectedServerName = server.Name;
        SelectedServerName = server.Name;
        SelectRequested?.Invoke(server);
    }

    public void DeleteServer(Server server)
    {
        DataStore.Instance.Data.Servers.Remove(server);
        if (DataStore.Instance.SelectedServerName == server.Name)
            DataStore.Instance.SelectedServerName = null;
        DataStore.Instance.Save();
        Refresh();
    }
}
