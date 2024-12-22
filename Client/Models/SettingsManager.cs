using System.Text.Json;

namespace Client.Models;

public static class SettingsManager
{
    private const string SettingsFile = "settings.json";
    public static event Action? SettingsChanged;

    public static void SaveSettings(Settings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(SettingsFile, json);
        SettingsChanged?.Invoke();
    }

    public static Settings LoadSettings()
    {
        if (File.Exists(SettingsFile))
        {
            var json = File.ReadAllText(SettingsFile);
            return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
        }

        return new Settings();
    }
}