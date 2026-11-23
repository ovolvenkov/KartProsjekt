using Microsoft.UI.Xaml;
using KartProsjekt.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace KartProsjekt;

/// <summary>
/// The application window. This hosts a Frame that displays pages. Add your
/// UI and logic to MainPage.xaml / MainPage.xaml.cs instead of here so you
/// can use Page features such as navigation events and the Loaded lifecycle.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        AppWindow.SetIcon("Assets/AppIcon.ico");

        var settings = AppSettingsService.Instance;
        settings.SettingsChanged += OnSettingsChanged;
        ApplyTheme(settings.Theme);

        // Navigate the root frame to the main page on startup.
        RootFrame.Navigate(typeof(MainPage));
    }

    private void OnSettingsChanged(object? sender, EventArgs args)
    {
        ApplyTheme(AppSettingsService.Instance.Theme);
    }

    private void ApplyTheme(string theme)
    {
        // Set the theme at the window root so navigation and hosted pages inherit it.
        WindowRoot.RequestedTheme = theme switch
        {
            "Light" => ElementTheme.Light,
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Default
        };
    }
}
