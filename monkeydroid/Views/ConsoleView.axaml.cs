using System.Collections.Specialized;
using Avalonia.Controls;
using monkeydroid.ViewModels;

namespace monkeydroid.Views;

public partial class ConsoleView : UserControl
{
    public ConsoleView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is ConsoleViewModel vm)
        {
            vm.OutputLines.CollectionChanged += OnOutputChanged;
        }
    }

    private void OnOutputChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Auto-scroll to bottom when new output is added
        OutputScroller.ScrollToEnd();
    }
}
