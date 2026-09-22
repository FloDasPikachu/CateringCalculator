using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace CateringCalculator.UI.Platforms.Android;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity {
    protected override void OnCreate(Bundle? savedInstanceState) {
        base.OnCreate(savedInstanceState);

        // Verhindert, dass die App-Inhalte unter die Statusleiste/Notch geschoben werden
        if (Window != null) {
            Window.SetFlags(WindowManagerFlags.DrawsSystemBarBackgrounds, WindowManagerFlags.DrawsSystemBarBackgrounds);
            Window.DecorView.SetFitsSystemWindows(true);
        }
    }
}