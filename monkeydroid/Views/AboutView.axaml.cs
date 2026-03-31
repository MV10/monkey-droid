using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using monkeydroid.ViewModels;

namespace monkeydroid.Views;

public partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
        OkButton.Click += OnOkClick;
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        var mainView = this.FindAncestorOfType<MainView>();
        if (mainView?.DataContext is MainViewModel vm)
            vm.ReturnFromNonSequenceView();
    }
}
