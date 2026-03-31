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
    }

    private async void OnItemClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not Button button) return;
        if (button.DataContext is not VisualizerInfo viz) return;
        if (DataContext is not VisualizersViewModel vm) return;
        await vm.SelectVisualizerCommand.ExecuteAsync(viz);
    }
}
