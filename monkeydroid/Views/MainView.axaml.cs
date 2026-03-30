using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using monkeydroid.Services;
using monkeydroid.ViewModels;

namespace monkeydroid.Views;

public partial class MainView : UserControl
{
    private bool _overlayVisible;
    private Point _pointerPressedPoint;
    private const double SwipeThreshold = 80;

    public MainView()
    {
        InitializeComponent();

        AddHandler(PointerPressedEvent, OnPointerPressedTunnel, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, OnPointerReleasedTunnel, RoutingStrategies.Tunnel);

        HamburgerButton.Click += OnHamburgerClick;

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;

        vm.InitializePages();
        vm.ShowSplash();

        DataStore.Instance.Load();

        await Task.Delay(5000);

        vm.StartupNavigation();
    }

    // --- Swipe navigation ---

    private void OnPointerPressedTunnel(object? sender, PointerPressedEventArgs e)
    {
        if (_overlayVisible) return;
        _pointerPressedPoint = e.GetPosition(this);
    }

    private void OnPointerReleasedTunnel(object? sender, PointerReleasedEventArgs e)
    {
        if (_overlayVisible) return;
        if (DataContext is not MainViewModel vm) return;
        if (vm.IsSplashActive) return;

        var released = e.GetPosition(this);
        var deltaX = released.X - _pointerPressedPoint.X;

        if (Math.Abs(deltaX) >= SwipeThreshold)
        {
            if (deltaX < 0)
                vm.GoForwardCommand.Execute(null);
            else
                vm.GoBackCommand.Execute(null);
        }
    }

    // --- Hamburger menu ---

    private void OnHamburgerClick(object? sender, RoutedEventArgs e)
    {
        if (_overlayVisible) return;
        if (DataContext is not MainViewModel vm) return;

        ShowMenuOverlay(new[]
        {
            "Help",
            "Docs",
            "Get support",
            "About",
            "",
            vm.AutoSelectServer ? "\u2713 Auto-select server" : "\u2022 Auto-select server",
            "",
            "Reset",
        }, menuItem =>
        {
            var item = menuItem.TrimStart('\u2713', '\u2022', ' ');
            switch (item)
            {
                case "Auto-select server":
                    vm.ToggleAutoSelect();
                    break;
                case "Help":
                    vm.ShowNonSequenceView(new HelpViewModel());
                    break;
                case "About":
                    vm.ShowNonSequenceView(new AboutViewModel());
                    break;
                case "Docs":
                    LaunchUri("https://www.monkeyhihat.com/docs/index.php#/using-monkey-hi-hat?id=remote-control-monkey-droid-gui");
                    break;
                case "Get support":
                    LaunchUri("https://www.monkeyhihat.com/docs/index.php#/troubleshooting");
                    break;
                case "Reset":
                    ShowPromptOverlay("Reset all data and restart?", confirmed =>
                    {
                        if (!confirmed) return;
                        DataStore.Instance.Reset();
                        vm.AutoSelectServer = false;
                        vm.StartupNavigation();
                    });
                    break;
            }
        });
    }

    private async void LaunchUri(string uri)
    {
        try
        {
            if (OperatingSystem.IsAndroid())
            {
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel?.Launcher is not null)
                    await topLevel.Launcher.LaunchUriAsync(new Uri(uri));
            }
            else
            {
                LaunchUriDesktop(uri);
            }
        }
        catch
        {
            // Silently ignore if the platform can't launch URIs
        }
    }

    // Isolated to a separate method so the Android trimmer/AOT doesn't
    // try to resolve System.Diagnostics.Process when loading MainView.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void LaunchUriDesktop(string uri)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(uri)
        {
            UseShellExecute = true
        });
    }

    // --- Overlay: Simple message (Ok button) ---

    public void ShowMessageOverlay(string message, bool centerText = false)
    {
        _overlayVisible = true;

        var messageText = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = centerText ? TextAlignment.Center : TextAlignment.Left,
            FontSize = 16,
            Margin = new Thickness(0, 0, 0, 20),
        };

        var okButton = new Button
        {
            Content = "Ok",
            HorizontalAlignment = HorizontalAlignment.Center,
            MinWidth = 100,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Classes = { "accent" },
        };

        var dialog = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(32),
            MaxWidth = 320,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new StackPanel
            {
                Children = { messageText, okButton },
            },
        };

        var overlay = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
            Child = dialog,
        };

        okButton.Click += (_, _) =>
        {
            OverlayHost.Children.Remove(overlay);
            _overlayVisible = false;
        };

        OverlayHost.Children.Add(overlay);
    }

    // --- Overlay: Prompt (Ok / Cancel) ---

    public void ShowPromptOverlay(string message, Action<bool> callback)
    {
        _overlayVisible = true;

        var messageText = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 16,
            Margin = new Thickness(0, 0, 0, 20),
        };

        var okButton = new Button
        {
            Content = "Ok",
            MinWidth = 90,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            Classes = { "accent" },
        };

        var cancelButton = new Button
        {
            Content = "Cancel",
            MinWidth = 90,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 16,
            Children = { okButton, cancelButton },
        };

        var dialog = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(32),
            MaxWidth = 320,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new StackPanel
            {
                Children = { messageText, buttonPanel },
            },
        };

        var overlay = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
            Child = dialog,
        };

        void Dismiss(bool result)
        {
            OverlayHost.Children.Remove(overlay);
            _overlayVisible = false;
            callback(result);
        }

        okButton.Click += (_, _) => Dismiss(true);
        cancelButton.Click += (_, _) => Dismiss(false);

        OverlayHost.Children.Add(overlay);
    }

    // --- Overlay: Menu list (tap outside to dismiss) ---

    public void ShowMenuOverlay(string[] items, Action<string> onSelected, bool centered = false)
    {
        _overlayVisible = true;

        var menuStack = new StackPanel
        {
            Spacing = 2,
        };

        var dialog = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(8),
            MinWidth = 220,
            HorizontalAlignment = centered ? HorizontalAlignment.Center : HorizontalAlignment.Left,
            VerticalAlignment = centered ? VerticalAlignment.Center : VerticalAlignment.Top,
            Margin = centered ? new Thickness(0) : new Thickness(8, 8, 0, 0),
            Child = menuStack,
        };

        var overlay = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)),
            Child = dialog,
        };

        void Dismiss()
        {
            OverlayHost.Children.Remove(overlay);
            _overlayVisible = false;
        }

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item))
            {
                menuStack.Children.Add(new Border
                {
                    Height = 1,
                    Background = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                    Margin = new Thickness(8, 4),
                });
                continue;
            }

            var menuButton = new Button
            {
                Content = item,
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(12, 8),
                FontSize = 15,
            };

            var capturedItem = item;
            menuButton.Click += (_, _) =>
            {
                Dismiss();
                onSelected(capturedItem);
            };

            menuStack.Children.Add(menuButton);
        }

        overlay.PointerPressed += (_, args) =>
        {
            if (args.Source == overlay)
            {
                Dismiss();
            }
        };

        OverlayHost.Children.Add(overlay);
    }
}
