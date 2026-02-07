using System.Diagnostics;
using System.IO;
using System.Text.Json;
using TimesheetTracker.Core;

namespace TimesheetTracker.WPF.Configuration;

public static class AppConfiguration
{
    private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public static DirectoryInfo TimesheetDirectory { get; } = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TimesheetTracker"));

    private static Timesheet ConfigTemplate()
    {
        var timesheet = new Timesheet(DateTime.Now.Year, DateTime.Now.Month);

        for (int i = 0; i <= 5; i++)
        {
            _ = timesheet.CreateProject($"Project {i}", 20);
        }

        return timesheet;
    }

    [Obsolete]
    public static void ShowJsonConfig()
    {
        using var _ = Process.Start("explorer.exe", TimesheetDirectory.FullName);
    }

    public static string TimesheetToJson(Timesheet timesheet)
    {
        return JsonSerializer.Serialize(timesheet, _serializerOptions);
    }

    public static async Task<(bool hasError, Timesheet? timesheet)> LoadTimesheet(int year, int month)
    {
        FileInfo timesheetConfig = new(Path.Combine(TimesheetDirectory.FullName, $"timesheet_{year}_{month}.json"));

        try
        {
            using var fileStream = timesheetConfig.OpenRead();
            Timesheet? config = await JsonSerializer.DeserializeAsync<Timesheet>(fileStream);
            return (true, config);
        }
        catch (JsonException)
        {
            Timesheet config = ConfigTemplate();
            return (true, config);
        }
        catch (FileNotFoundException)
        {
            var configCreation = timesheetConfig.Create().DisposeAsync();
            string timesheetJson = TimesheetToJson(ConfigTemplate());
            File.WriteAllText(timesheetConfig.FullName, timesheetJson);

            await configCreation;
            return await LoadTimesheet(year, month);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
