using CommunityToolkit.Mvvm.ComponentModel;
using monkeydroid.Services;

namespace monkeydroid.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty] private string _serverLabel = "";

    public void RefreshServerLabel()
    {
        ServerLabel = DataStore.Instance.SelectedServerName ?? "";
    }
}