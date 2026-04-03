using System.Collections.Specialized;
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
        DataContextChanged += (_, _) =>
        {
            if (DataContext is PlaylistsViewModel vm)
                vm.Playlists.CollectionChanged += OnCollectionChanged;
        };
        Loaded += (_, _) =>
        {
            if (DataContext is PlaylistsViewModel vm)
                ListScroller.Offset = new Avalonia.Vector(0, vm.ScrollOffset);
        };
        Unloaded += (_, _) =>
        {
            if (DataContext is PlaylistsViewModel vm)
                vm.ScrollOffset = ListScroller.Offset.Y;
        };
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add
            && DataContext is PlaylistsViewModel { IsNotDownloading: false })
            ListScroller.ScrollToEnd();
    }

    private async void OnItemClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Button button) return;
        if (button.DataContext is not PlaylistInfo playlist) return;
        if (DataContext is not PlaylistsViewModel vm) return;
        await vm.SelectPlaylistCommand.ExecuteAsync(playlist);
    }
}
