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
    }

    private async void OnItemClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Button button) return;
        if (button.DataContext is not FxInfo fx) return;
        if (DataContext is not FxViewModel vm) return;
        await vm.SelectFxCommand.ExecuteAsync(fx);
    }
}
