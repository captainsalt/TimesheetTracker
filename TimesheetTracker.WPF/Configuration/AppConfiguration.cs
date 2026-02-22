using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using TimesheetTracker.Core;

namespace TimesheetTracker.WPF.Configuration;

public static class AppConfiguration
{
    private static readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };
    private static Regex TimesheetRegex => new(@"timesheet_(?<year>\d+)_(?<month>\d+)\.json$");

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

    private static string TimesheetFileName(int year, int month)
    {
        return $"timesheet_{year}_{month}.json";
    }

    public static void OpenTimesheetDirectory()
    {
        using var _ = Process.Start("explorer.exe", TimesheetDirectory.FullName);
    }

    public static string TimesheetToJson(Timesheet timesheet)
    {
        return JsonSerializer.Serialize(timesheet, _serializerOptions);
    }

    public static async Task SaveTimesheet(Timesheet timesheet)
    {
        if (!TimesheetDirectory.Exists)
            TimesheetDirectory.Create();

        string timesheetJson = TimesheetToJson(timesheet);
        FileInfo timesheetConfig = new(Path.Combine(TimesheetDirectory.FullName, TimesheetFileName(timesheet.Year, timesheet.Month)));
        await File.WriteAllTextAsync(timesheetConfig.FullName, timesheetJson);
    }

    public static async Task<(Exception? exception, Timesheet? timesheet)> LoadTimesheet(string fileName)
    {
        GroupCollection matchCollection = TimesheetRegex.Match(fileName).Groups;
        bool yearMatched = int.TryParse(matchCollection["year"].Value, out int year);
        bool monthMatched = int.TryParse(matchCollection["month"].Value, out int month);

        return yearMatched && monthMatched
            ? await LoadTimesheet(year, month)
            : (new FormatException("File name does not match expected format"), ConfigTemplate());
    }

    public static async Task<(Exception? exception, Timesheet? timesheet)> LoadTimesheet(int year, int month)
    {
        FileInfo timesheetConfig = new(Path.Combine(TimesheetDirectory.FullName, TimesheetFileName(year, month)));

        try
        {
            using FileStream fileStream = timesheetConfig.OpenRead();
            Timesheet? config = await JsonSerializer.DeserializeAsync<Timesheet>(fileStream);
            return (null, config);
        }
        catch (JsonException ex)
        {
            Timesheet config = ConfigTemplate();
            return (ex, config);
        }
        catch (FileNotFoundException)
        {
            string timesheetJson = TimesheetToJson(ConfigTemplate());
            File.WriteAllText(timesheetConfig.FullName, timesheetJson);

            return await LoadTimesheet(year, month);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
