using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using TimesheetTracker.Core;

namespace TimesheetTracker.WPF.Configuration;

public class AppConfiguration
{
    private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public static DirectoryInfo SettingsPath { get; } = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TimesheetTracker"));

    private static async Task InitConfig()
    {
        if (!SettingsPath.Exists)
            SettingsPath.Create();
    }

    private static Timesheet ConfigTemplate()
    {
        return new Timesheet();
    }

    public static void ShowJsonConfig()
    {
        using var _ = Process.Start("explorer.exe", SettingsPath.FullName);
    }

    public static string SaveTimesheet(Timesheet timesheet)
    {
        return JsonSerializer.Serialize(timesheet, _serializerOptions);
    }

    public static async Task<(bool hasError, Timesheet? config)> LoadTimesheet(FileInfo timesheetConfig)
    {
        try
        {
            var config = await JsonSerializer.DeserializeAsync<Timesheet>(timesheetConfig.OpenRead());
            return (false, config);
        }
        catch (JsonException)
        {
            var config = ConfigTemplate();
            return (true, config);
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
