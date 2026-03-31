using Avalonia.Controls;
using Avalonia.Interactivity;
using monkeydroid.Models;
using monkeydroid.ViewModels;

namespace monkeydroid.Views;

public partial class PlaylistsView : UserControl
{
    public PlaylistsView()
    {
        InitializeComponent();
        AddHandler(Button.ClickEvent, OnItemClick, RoutingStrategies.Bubble);
    }

    private async void OnItemClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Button button) return;
        if (button.DataContext is not PlaylistInfo playlist) return;
        if (DataContext is not PlaylistsViewModel vm) return;
        await vm.SelectPlaylistCommand.ExecuteAsync(playlist);
    }
}
