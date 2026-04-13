using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace monkeydroid.Android;

[Application]
public class MonkeyDroidApplication : AvaloniaAndroidApplication<App>
{
    public MonkeyDroidApplication(nint handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
