using Avalonia.Controls;
using Avalonia.VisualTree;
using monkeydroid.ViewModels;

namespace monkeydroid.Views;

public partial class CommonControlsView : UserControl
{
    public CommonControlsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is CommonControlsViewModel vm)
        {
            vm.ShowInfoResponse += response =>
            {
                var mainView = this.FindAncestorOfType<MainView>();
                mainView?.ShowMessageOverlay(response);
            };
        }
    }
}
