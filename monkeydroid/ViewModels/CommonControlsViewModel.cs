using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using monkeydroid.Services;

namespace monkeydroid.ViewModels;

public partial class CommonControlsViewModel : ViewModelBase
{
    public string Title => "Common Controls";

    public event System.Action<string>? ShowInfoResponse;

    [RelayCommand]
    private async Task What()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;
        await CommsService.SendCommand(server, "--show", "what");
    }

    [RelayCommand]
    private async Task Standby()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;
        await CommsService.SendCommand(server, "--standby");
    }

    [RelayCommand]
    private async Task Fullscreen()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;
        await CommsService.SendCommand(server, "--fullscreen");
    }

    [RelayCommand]
    private async Task Fps()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;
        if (await CommsService.SendCommand(server, "--fps"))
        {
            var response = CommsService.GetResponse();
            if (!response.StartsWith("ERR", System.StringComparison.Ordinal))
                ShowInfoResponse?.Invoke(response);
        }
    }

    [RelayCommand]
    private async Task Track()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;
        await CommsService.SendCommand(server, "--show", "track");
    }

    [RelayCommand]
    private async Task Quit()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;
        await CommsService.SendCommand(server, "--quit");
    }

    [RelayCommand]
    private async Task Idle()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;
        await CommsService.SendCommand(server, "--idle");
    }

    [RelayCommand]
    private async Task Info()
    {
        var server = DataStore.Instance.GetSelectedServer();
        if (server is null) return;
        if (await CommsService.SendCommand(server, "--info"))
        {
            var response = CommsService.GetResponse();
            if (!response.StartsWith("ERR", System.StringComparison.Ordinal))
                ShowInfoResponse?.Invoke(response);
        }
    }
}
