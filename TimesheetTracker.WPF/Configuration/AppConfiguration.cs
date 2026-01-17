using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Navigation;

namespace TimesheetTracker.WPF.Configuration;

public record ProjectConfig(
    [property: JsonRequired] string Name,
    [property: JsonRequired] decimal MaxHours,
    decimal? CurrentHours,
    decimal? DailyMinimum);
public record Config(
    [property: JsonRequired] List<ProjectConfig> Projects,
    [property: JsonRequired] List<int> ExcludedDays);

public class AppConfiguration
{
    private static readonly FileInfo _settingsPath = new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "TimesheetTracker",
        "settings.json"));

    private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    static AppConfiguration()
    {
        if (_settingsPath.Exists == false)
            InitConfig();
    }

    private static void InitConfig()
    {
        if (_settingsPath.Directory is { Exists: false } directory)
            directory.Create();

        using var writer = _settingsPath.CreateText();
        writer.Write(JsonSerializer.Serialize(ConfigTemplate("Placeholder Project"), _serializerOptions));
    }

    private static Config ConfigTemplate(string projectName)
    {
        return new Config(
            Projects: [
                new ProjectConfig(projectName, MaxHours: 10, CurrentHours: 0, DailyMinimum: 0),
            ],
            ExcludedDays: []
        );
    }

    public static FileInfo SettingsPath => _settingsPath;

    public static void ShowJsonConfig() => Process.Start("explorer.exe", SettingsPath.Directory!.FullName);

    public static bool TryReadConfig(out Config? config)
    {
        try
        {
            config =  JsonSerializer.Deserialize<Config>(File.ReadAllText(SettingsPath.FullName));
            return true;
        }
        catch (JsonException)
        {
            config =  ConfigTemplate("<ERROR LOADING CONFIG>");
            return false;
        }
    }
}
