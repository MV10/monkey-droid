using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Interactivity;
using monkeydroid.Models;
using monkeydroid.ViewModels;

namespace monkeydroid.Views;

public partial class VisualizersView : UserControl
{
    public VisualizersView()
    {
        InitializeComponent();
        AddHandler(Button.ClickEvent, OnItemClick, RoutingStrategies.Bubble);
        DataContextChanged += (_, _) =>
        {
            if (DataContext is VisualizersViewModel vm)
                vm.Visualizers.CollectionChanged += OnCollectionChanged;
        };
        Loaded += (_, _) =>
        {
            if (DataContext is VisualizersViewModel vm)
                ListScroller.Offset = new Avalonia.Vector(0, vm.ScrollOffset);
        };
        Unloaded += (_, _) =>
        {
            if (DataContext is VisualizersViewModel vm)
                vm.ScrollOffset = ListScroller.Offset.Y;
        };
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add
            && DataContext is VisualizersViewModel { IsNotDownloading: false })
            ListScroller.ScrollToEnd();
    }

    private async void OnItemClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Button button) return;
        if (button.DataContext is not VisualizerInfo viz) return;
        if (DataContext is not VisualizersViewModel vm) return;
        await vm.SelectVisualizerCommand.ExecuteAsync(viz);
    }
}
