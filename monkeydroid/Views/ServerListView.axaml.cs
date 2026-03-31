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
        var menuItems = new[] { "Select", "Test", "Edit", autoSelectLabel, "Delete", "", "Cancel" };

        mainView.ShowMenuOverlay(menuItems, async selected =>
        {
            var vm = mainView.DataContext as MainViewModel;
            switch (selected)
            {
                case "Select":
                    slvm.RequestSelect(server);
                    vm?.NavigateToPlaylistsAfterSelect();
                    break;
                case "Test":
                    var connected = await CommsService.TryConnect(server);
                    mainView.ShowMessageOverlay(connected ? "Success" : "Failed");
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
