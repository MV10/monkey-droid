using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Interactivity;
using monkeydroid.Models;
using monkeydroid.ViewModels;

namespace monkeydroid.Views;

public partial class FxView : UserControl
{
    public FxView()
    {
        InitializeComponent();
        AddHandler(Button.ClickEvent, OnItemClick, RoutingStrategies.Bubble);
        DataContextChanged += (_, _) =>
        {
            if (DataContext is FxViewModel vm)
                vm.FxList.CollectionChanged += OnCollectionChanged;
        };
        Loaded += (_, _) =>
        {
            if (DataContext is FxViewModel vm)
                ListScroller.Offset = new Avalonia.Vector(0, vm.ScrollOffset);
        };
        Unloaded += (_, _) =>
        {
            if (DataContext is FxViewModel vm)
                vm.ScrollOffset = ListScroller.Offset.Y;
        };
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add
            && DataContext is FxViewModel { IsNotDownloading: false })
            ListScroller.ScrollToEnd();
    }

    private async void OnItemClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Button button) return;
        if (button.DataContext is not FxInfo fx) return;
        if (DataContext is not FxViewModel vm) return;
        await vm.SelectFxCommand.ExecuteAsync(fx);
    }
}
