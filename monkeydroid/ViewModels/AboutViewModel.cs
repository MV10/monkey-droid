namespace monkeydroid.ViewModels;

public class AboutViewModel : ViewModelBase
{
    public string Title => "About";

    public string VersionText =>
        $"Version {typeof(AboutViewModel).Assembly.GetName().Version?.ToString(3) ?? "?"}";
}
