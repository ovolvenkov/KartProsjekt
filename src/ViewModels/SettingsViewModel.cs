using CommunityToolkit.Mvvm.ComponentModel;
using KartProsjekt.Services;

namespace KartProsjekt.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppSettingsService _settings;

    public string[] ThemeOptions { get; } = ["System", "Light", "Dark"];

    [ObservableProperty]
    public partial string SelectedTheme { get; set; } = "System";

    [ObservableProperty]
    public partial bool IsCompactNavigation { get; set; }

    public SettingsViewModel(AppSettingsService settings)
    {
        _settings = settings;
        SelectedTheme = settings.Theme;
        IsCompactNavigation = settings.IsCompactNavigation;
    }

    partial void OnSelectedThemeChanged(string value)
    {
        _settings.SetTheme(value);
    }

    partial void OnIsCompactNavigationChanged(bool value)
    {
        _settings.SetCompactNavigation(value);
    }
}
