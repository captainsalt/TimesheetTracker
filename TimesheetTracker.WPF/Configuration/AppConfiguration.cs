using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimesheetTracker.WPF.Configuration;

public record ProjectConfig(
    [property: JsonRequired] string Name,
    [property: JsonRequired] int MaxHours,
    int? CurrentHours);
public record Config(
    [property: JsonRequired] List<ProjectConfig> Projects,
    [property: JsonRequired] List<int> ExcludedDays);

public class AppConfiguration
{
    private static readonly FileInfo _settingsPath = new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "TimesheetTracker",
        "settings.json"));

    private static JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    static AppConfiguration()
    {
        if (_settingsPath.Exists)
            return;

        InitConfig();
    }

    private static void InitConfig()
    {
        if (_settingsPath.Directory is { Exists: false } directory)
            directory.Create();

        using var writer = _settingsPath.CreateText();
        writer.Write(JsonSerializer.Serialize(DefaultConfig(), _serializerOptions));
    }

    private static Config DefaultConfig()
    {
        return new Config(
            Projects: [
                new ProjectConfig("Project A", MaxHours: 88, CurrentHours: 0),
                new ProjectConfig("Project B", MaxHours: 88, CurrentHours: 0),
            ],
            ExcludedDays: []
        );
    }

    public static FileInfo SettingsPath => _settingsPath;

    public static void ShowJsonConfig() => Process.Start("explorer.exe", SettingsPath.Directory!.FullName);

    public static Config? GetConfig()
    {
        try
        {
            return JsonSerializer.Deserialize<Config>(File.ReadAllText(SettingsPath.FullName));
        }
        catch (JsonException)
        {
            return DefaultConfig();
        }
    }
}
