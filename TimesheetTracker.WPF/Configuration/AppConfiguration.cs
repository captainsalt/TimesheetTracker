using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace TimesheetTracker.WPF.Configuration;

public record ProjectConfig(string Name, int MaxHours, int? CurrentHours);
public record Config(List<ProjectConfig> Projects, List<int> ExcludedDays);

public class AppConfiguration
{
    private static readonly FileInfo _settingsPath = new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "TimesheetTracker",
        "settings.json"));

    static AppConfiguration()
    {
        if (_settingsPath.Exists)
            return;

        if (_settingsPath.Directory is { Exists: false } directory)
            directory.Create();

        var defaultSettings = new Config(
            Projects: [
                new ProjectConfig("Project A", MaxHours: 88, CurrentHours: 0),
                new ProjectConfig("Project B", MaxHours: 88, CurrentHours: 0)
            ],
            ExcludedDays: []
        );

        using var writer = _settingsPath.CreateText();
        writer.Write(JsonSerializer.Serialize(defaultSettings));
    }

    public static FileInfo SettingsPath => _settingsPath;

    public static void ShowJsonConfig() => Process.Start("explorer.exe", SettingsPath.Directory!.FullName);

    public static Config? GetConfig() => JsonSerializer.Deserialize<Config>(File.ReadAllText(SettingsPath.FullName));
}
