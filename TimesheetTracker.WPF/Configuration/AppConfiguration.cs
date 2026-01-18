using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    static AppConfiguration()
    {
        if (SettingsPath.Exists == false)
            InitConfig();
    }

    private static void InitConfig()
    {
        if (SettingsPath.Directory is { Exists: false } directory)
            directory.Create();

        using StreamWriter writer = SettingsPath.CreateText();
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

    public static FileInfo SettingsPath { get; } = new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "TimesheetTracker",
        "settings.json"));

    public static void ShowJsonConfig()
    {
        using var _ = Process.Start("explorer.exe", SettingsPath.Directory!.FullName);
    }

    public static (bool hasError, Config? config) GetConfig()
    {
        try
        {
            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(SettingsPath.FullName));
            return (false, config);
        }
        catch (JsonException)
        {
            var config = ConfigTemplate("<ERROR LOADING CONFIG>");
            return (true, config);
        }
        catch (FileNotFoundException)
        {
            InitConfig();
            return GetConfig();
        }
        catch (Exception)
        {
            throw;
        }
    }
}
