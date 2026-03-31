using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using monkeydroid.Models;
using monkeydroid.Services;

namespace monkeydroid.ViewModels;

public partial class ServerEditorViewModel : ViewModelBase
{
    [ObservableProperty] private string _serverName = "";
    [ObservableProperty] private string _portText = "50001";
    [ObservableProperty] private string _altPortText = "50002";
    [ObservableProperty] private bool _canSave;
    [ObservableProperty] private string _nameWarning = "";
    [ObservableProperty] private string _portWarning = "";
    [ObservableProperty] private string _altPortWarning = "";

    public bool IsAddMode { get; }
    public string Title => IsAddMode ? "Add Server" : "Edit Server";
    public bool CanCancel => DataStore.Instance.HasServers;

    public event Action? SaveRequested;
    public event Action? CancelRequested;

    private readonly string? _originalName;

    public ServerEditorViewModel(bool isAddMode, Server? existing = null)
    {
        IsAddMode = isAddMode;

        if (existing is not null)
        {
            _originalName = existing.Name;
            ServerName = existing.Name;
            PortText = existing.Port.ToString();
            AltPortText = existing.AlternatePort?.ToString() ?? "";
        }
    }

    partial void OnServerNameChanged(string value) => Validate();
    partial void OnPortTextChanged(string value) => Validate();
    partial void OnAltPortTextChanged(string value) => Validate();

    private void Validate()
    {
        NameWarning = ServerName.Length > 15
            ? "Names over 15 characters may not be compatible with all networks"
            : "";

        var portValid = int.TryParse(PortText, out var port) && port >= 1 && port <= 65535;
        PortWarning = portValid && port < 49152
            ? "Ports below 49152 may interfere with reserved ports"
            : "";

        var altValid = string.IsNullOrWhiteSpace(AltPortText);
        if (!altValid)
        {
            altValid = int.TryParse(AltPortText, out var altPort) && altPort >= 1 && altPort <= 65535;
            AltPortWarning = altValid && int.Parse(AltPortText) < 49152
                ? "Ports below 49152 may interfere with reserved ports"
                : "";
        }
        else
        {
            AltPortWarning = "";
        }

        CanSave = !string.IsNullOrWhiteSpace(ServerName)
                  && ServerName.Length <= 63
                  && portValid
                  && altValid;
    }

    [RelayCommand]
    private void Save()
    {
        if (!CanSave) return;

        var store = DataStore.Instance;
        var port = int.Parse(PortText);
        int? altPort = string.IsNullOrWhiteSpace(AltPortText) ? null : int.Parse(AltPortText);

        if (IsAddMode)
        {
            store.Data.Servers.Add(new Server
            {
                Name = ServerName.Trim(),
                Port = port,
                AlternatePort = altPort,
            });
        }
        else
        {
            var server = store.Data.Servers.FirstOrDefault(s =>
                s.Name.Equals(_originalName, StringComparison.OrdinalIgnoreCase));
            if (server is not null)
            {
                server.Name = ServerName.Trim();
                server.Port = port;
                server.AlternatePort = altPort;
            }
        }

        store.Save();
        SaveRequested?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CancelRequested?.Invoke();
    }
}
