using System.Diagnostics;
using System.IO;
using System.Text.Json;
using TimesheetTracker.Core;

namespace TimesheetTracker.WPF.Configuration;

public class AppConfiguration
{
    private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    [Obsolete]
    public static DirectoryInfo SettingsDirectory { get; } = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TimesheetTracker"));

    private static Timesheet ConfigTemplate()
    {
        var timesheet = new Timesheet(DateTime.Now.Year, DateTime.Now.Month);

        for (int i = 0; i < 5; i++)
        {
            _ = timesheet.CreateProject($"Project {i}", 20);
        }

        return timesheet;
    }

    [Obsolete]
    public static void ShowJsonConfig()
    {
        using var _ = Process.Start("explorer.exe", SettingsDirectory.FullName);
    }

    public static string TimesheetToJson(Timesheet timesheet)
    {
        return JsonSerializer.Serialize(timesheet, _serializerOptions);
    }

    public static async Task<(bool hasError, Timesheet? config)> LoadTimesheet(FileInfo timesheetConfig)
    {
        try
        {
            Timesheet? config = await JsonSerializer.DeserializeAsync<Timesheet>(timesheetConfig.OpenRead());
            return (false, config);
        }
        catch (JsonException)
        {
            Timesheet config = ConfigTemplate();
            return (true, config);
        }
        catch (FileNotFoundException)
        {
            if (!timesheetConfig.Directory!.Exists)
            {
                timesheetConfig.Directory.Create();
            }

            string timesheetJson = TimesheetToJson(ConfigTemplate());
            await File.WriteAllTextAsync(timesheetConfig.FullName, timesheetJson);
            return LoadTimesheet(timesheetConfig).Result;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
