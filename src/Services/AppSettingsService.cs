using System.Text.Json;

namespace KartProsjekt.Services;

public sealed class AppSettingsService
{
    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KartProsjekt",
        "settings.json");

    public static AppSettingsService Instance { get; } = new();

    public string Theme { get; private set; }
    public bool IsCompactNavigation { get; private set; }

    public event EventHandler? SettingsChanged;

    private AppSettingsService()
    {
        if (!File.Exists(SettingsFilePath))
        {
            Theme = "System";
            IsCompactNavigation = false;
            return;
        }

        var settings = JsonSerializer.Deserialize<SettingsData>(
            File.ReadAllText(SettingsFilePath))
            ?? throw new InvalidDataException("The KartProsjekt settings file is empty.");

        if (settings.Theme is not ("System" or "Light" or "Dark"))
        {
            throw new InvalidDataException("The KartProsjekt theme setting is invalid.");
        }

        Theme = settings.Theme;
        IsCompactNavigation = settings.IsCompactNavigation;
    }

    public void SetTheme(string theme)
    {
        if (theme is not ("System" or "Light" or "Dark"))
        {
            throw new ArgumentOutOfRangeException(nameof(theme), theme, "Unsupported theme.");
        }

        if (Theme == theme)
        {
            return;
        }

        Save(theme, IsCompactNavigation);
        Theme = theme;
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetCompactNavigation(bool isCompactNavigation)
    {
        if (IsCompactNavigation == isCompactNavigation)
        {
            return;
        }

        Save(Theme, isCompactNavigation);
        IsCompactNavigation = isCompactNavigation;
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private static void Save(string theme, bool isCompactNavigation)
    {
        var directory = Path.GetDirectoryName(SettingsFilePath)
            ?? throw new InvalidOperationException("The settings directory could not be determined.");

        Directory.CreateDirectory(directory);
        var settings = new SettingsData
        {
            Theme = theme,
            IsCompactNavigation = isCompactNavigation
        };

        File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(settings));
    }

    private sealed class SettingsData
    {
        public string Theme { get; set; } = "System";
        public bool IsCompactNavigation { get; set; }
    }
}
