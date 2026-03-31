using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using monkeydroid.Services;

namespace monkeydroid.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private ViewModelBase? _currentPage;
    [ObservableProperty] private string _currentTitle = "";
    [ObservableProperty] private bool _showNavigation;
    [ObservableProperty] private bool _showHamburgerMenu;
    [ObservableProperty] private bool _showNavArrows;
    [ObservableProperty] private bool _isSplashActive = true;
    [ObservableProperty] private string _autoSelectServer = "";

    private int _currentPageIndex;
    private ViewModelBase? _savedPage;
    private int _savedPageIndex;
    private bool _isNonSequenceView;

    public ObservableCollection<ViewModelBase> Pages { get; } = new();

    // Page VMs accessible for wiring events
    public ServerListViewModel ServerListVM { get; } = new();
    public PlaylistsViewModel PlaylistsVM { get; } = new();
    public CommonControlsViewModel CommonControlsVM { get; } = new();
    public VisualizersViewModel VisualizersVM { get; } = new();
    public FxViewModel FxVM { get; } = new();
    public ConsoleViewModel ConsoleVM { get; } = new();

    public void InitializePages()
    {
        Pages.Clear();
        Pages.Add(ServerListVM);
        Pages.Add(PlaylistsVM);
        Pages.Add(CommonControlsVM);
        Pages.Add(VisualizersVM);
        Pages.Add(FxVM);
        Pages.Add(ConsoleVM);
    }

    public bool HasServerSelected => DataStore.Instance.SelectedServerName is not null;

    public void ShowSplash()
    {
        IsSplashActive = true;
        ShowNavigation = false;
        ShowHamburgerMenu = false;
        CurrentTitle = "";
    }

    public void StartupNavigation()
    {
        IsSplashActive = false;
        AutoSelectServer = DataStore.Instance.Data.AutoSelectServer;

        if (!DataStore.Instance.HasServers)
        {
            ShowServerEditor(isAddMode: true);
            return;
        }

        if (!string.IsNullOrEmpty(AutoSelectServer))
        {
            var server = DataStore.Instance.Data.Servers.FirstOrDefault(s =>
                s.Name.Equals(AutoSelectServer, StringComparison.OrdinalIgnoreCase));
            if (server is not null)
            {
                DataStore.Instance.SelectedServerName = server.Name;
                ServerListVM.SelectedServerName = server.Name;
                NavigateToPage(1); // Playlists
                PlaylistsVM.LoadFromCache();
                return;
            }
        }

        NavigateToPage(0); // Server List
        ServerListVM.Refresh();
    }

    public void NavigateToPage(int index)
    {
        if (index < 0 || index >= Pages.Count) return;

        _isNonSequenceView = false;
        _currentPageIndex = index;
        CurrentPage = Pages[index];
        CurrentPage.RefreshServerLabel();
        UpdateTitle();
        ShowNavigation = true;
        ShowHamburgerMenu = true;
        ShowNavArrows = HasServerSelected;
    }

    private void UpdateTitle()
    {
        CurrentTitle = CurrentPage switch
        {
            ServerListViewModel vm => vm.Title,
            PlaylistsViewModel vm => vm.Title,
            CommonControlsViewModel vm => vm.Title,
            VisualizersViewModel vm => vm.Title,
            FxViewModel vm => vm.Title,
            ConsoleViewModel vm => vm.Title,
            AboutViewModel vm => vm.Title,
            HelpViewModel vm => vm.Title,
            ServerEditorViewModel vm => vm.Title,
            _ => "",
        };
    }

    [RelayCommand]
    private void GoBack()
    {
        if (_isNonSequenceView) return;
        if (!HasServerSelected && _currentPageIndex == 0) return;

        var newIndex = _currentPageIndex - 1;
        if (newIndex < 0) newIndex = Pages.Count - 1;

        // Can't navigate away from Server List without a selected server
        if (!HasServerSelected && newIndex != 0) return;

        NavigateToPage(newIndex);
        OnPageNavigated();
    }

    [RelayCommand]
    private void GoForward()
    {
        if (_isNonSequenceView) return;
        if (!HasServerSelected && _currentPageIndex == 0) return;

        var newIndex = _currentPageIndex + 1;
        if (newIndex >= Pages.Count) newIndex = 0;

        NavigateToPage(newIndex);
        OnPageNavigated();
    }

    private void OnPageNavigated()
    {
        // Refresh data when navigating to list views
        if (CurrentPage is PlaylistsViewModel pvm) pvm.LoadFromCache();
        else if (CurrentPage is VisualizersViewModel vvm) vvm.LoadFromCache();
        else if (CurrentPage is FxViewModel fvm) fvm.LoadFromCache();
        else if (CurrentPage is ServerListViewModel svm) svm.Refresh();
    }

    public void ShowNonSequenceView(ViewModelBase vm)
    {
        _savedPage = CurrentPage;
        _savedPageIndex = _currentPageIndex;
        _isNonSequenceView = true;
        CurrentPage = vm;
        UpdateTitle();
        ShowNavigation = true;
        ShowHamburgerMenu = vm is not ServerEditorViewModel;
        ShowNavArrows = false;
    }

    public void ReturnFromNonSequenceView()
    {
        _isNonSequenceView = false;

        if (_savedPage is not null)
        {
            CurrentPage = _savedPage;
            _currentPageIndex = _savedPageIndex;
            _savedPage = null;
            UpdateTitle();
        }
        else
        {
            NavigateToPage(0);
        }

        ShowHamburgerMenu = true;
    }

    public void ShowServerEditor(bool isAddMode, Models.Server? server = null)
    {
        var editor = new ServerEditorViewModel(isAddMode, server);
        editor.SaveRequested += () =>
        {
            ReturnFromNonSequenceView();
            ServerListVM.Refresh();
            NavigateToPage(0);
        };
        editor.CancelRequested += () =>
        {
            ReturnFromNonSequenceView();
        };
        ShowNonSequenceView(editor);
    }

    public void ClearAutoSelect()
    {
        AutoSelectServer = "";
        DataStore.Instance.Data.AutoSelectServer = "";
        DataStore.Instance.Save();
        ServerListVM.Refresh();
    }

    public void SetAutoSelect(string serverName)
    {
        AutoSelectServer = serverName;
        DataStore.Instance.Data.AutoSelectServer = serverName;
        DataStore.Instance.Save();
        ServerListVM.Refresh();
    }

    public void NavigateToPlaylistsAfterSelect()
    {
        NavigateToPage(1);
        PlaylistsVM.LoadFromCache();
    }
}
