using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;

namespace KeeMind;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private static MainActivity? MainActivityInstance { get; set; }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        MainActivityInstance = this;

        base.OnCreate(savedInstanceState);
    }

    public static void SetWindowTheme(bool isLight)
    {
        var window = MainActivityInstance?.Window;
        if (window == null)
        {
            return;

        }

        window.SetStatusBarColor((isLight ? KeeMind.Resources.AppAppTheme.Current.WhiteColor : KeeMind.Resources.AppAppTheme.Current.BlackColor).ToAndroid());
        window.SetNavigationBarColor((isLight ? KeeMind.Resources.AppAppTheme.Current.WhiteColor : KeeMind.Resources.AppAppTheme.Current.BlackColor).ToAndroid());

        var _ = new WindowInsetsControllerCompat(window, window.DecorView)
        {
            AppearanceLightStatusBars = !isLight,
            AppearanceLightNavigationBars = !isLight
        };
    }
}
