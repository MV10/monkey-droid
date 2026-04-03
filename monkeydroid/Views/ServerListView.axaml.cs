using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using monkeydroid.Models;
using monkeydroid.Services;
using monkeydroid.ViewModels;

namespace monkeydroid.Views;

public partial class ServerListView : UserControl
{
    public ServerListView()
    {
        InitializeComponent();
        AddButton.Click += OnAddClick;
        AddHandler(Button.ClickEvent, OnItemClick, RoutingStrategies.Bubble);
    }

    private void OnAddClick(object? sender, RoutedEventArgs e)
    {
        var mainView = this.FindAncestorOfType<MainView>();
        if (mainView?.DataContext is MainViewModel vm)
            vm.ShowServerEditor(isAddMode: true);
    }

    private void OnItemClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Button button) return;
        if (button == AddButton) return;
        if (button.DataContext is not Server server) return;
        if (DataContext is not ServerListViewModel slvm) return;

        var mainView = this.FindAncestorOfType<MainView>();
        if (mainView is null) return;

        var isAutoSelect = server.Name.Equals(DataStore.Instance.Data.AutoSelectServer, System.StringComparison.OrdinalIgnoreCase);
        var autoSelectLabel = isAutoSelect ? "Disable auto-select" : "Auto-select at startup";
        var testMain = $"Test :{server.Port}";
        var menuItems = new System.Collections.Generic.List<string>
            { $"@{server.Name}", "", "Use this server", testMain };
        if (server.AlternatePort.HasValue)
            menuItems.Add($"Test :{server.AlternatePort.Value}");
        menuItems.AddRange(new[] { "Edit", autoSelectLabel, "Delete", "", "Cancel" });

        mainView.ShowMenuOverlay(menuItems.ToArray(), async selected =>
        {
            var vm = mainView.DataContext as MainViewModel;
            switch (selected)
            {
                case "Use this server":
                    slvm.RequestSelect(server);
                    vm?.NavigateToPlaylistsAfterSelect();
                    break;
                case var s when s == testMain:
                    CommsService.PushBusy();
                    var connectTask = CommandLineSwitchPipe.CommandLineSwitchServer.TryConnect(server.Name, server.Port);
                    var connected = await Task.WhenAny(connectTask, Task.Delay(1000)) == connectTask && connectTask.Result;
                    CommsService.PopBusy();
                    mainView.ShowMessageOverlay($"Connection to {server.Name} port {server.Port} {(connected ? "succeeded" : "failed")}.");
                    break;
                case var s when s.StartsWith("Test :") && server.AlternatePort.HasValue:
                    CommsService.PushBusy();
                    var altTask = CommandLineSwitchPipe.CommandLineSwitchServer.TryConnect(server.Name, server.AlternatePort.Value);
                    var altConnected = await Task.WhenAny(altTask, Task.Delay(1000)) == altTask && altTask.Result;
                    CommsService.PopBusy();
                    mainView.ShowMessageOverlay($"Connection to {server.Name} port {server.AlternatePort.Value} {(altConnected ? "succeeded" : "failed")}.");
                    break;
                case "Edit":
                    vm?.ShowServerEditor(isAddMode: false, server);
                    break;
                case "Auto-select at startup":
                    vm?.SetAutoSelect(server.Name);
                    break;
                case "Disable auto-select":
                    vm?.ClearAutoSelect();
                    break;
                case "Delete":
                    mainView.ShowPromptOverlay(
                        $"Permanently delete server '{server.Name}'?",
                        confirmed =>
                        {
                            if (!confirmed) return;
                            slvm.DeleteServer(server);
                            if (!DataStore.Instance.HasServers)
                                vm?.ShowServerEditor(isAddMode: true);
                        });
                    break;
            }
        }, centered: true);
    }
}
