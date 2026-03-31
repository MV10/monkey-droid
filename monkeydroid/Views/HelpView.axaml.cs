using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using monkeydroid.ViewModels;

namespace monkeydroid.Views;

public partial class HelpView : UserControl
{
    public HelpView()
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
