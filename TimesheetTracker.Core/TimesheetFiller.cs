using System;

namespace TimesheetTracker.Core;

public static class TimesheetFiller
{
    public const int MAX_DAILY_HOURS = 8;
    public static readonly Random rng = new();

    public static Timesheet FillTimesheet(Timesheet timesheet)
    {
        ArgumentNullException.ThrowIfNull(timesheet);

        while (IncompleteDays(timesheet).Any())
        {
            var project = GetIncompleteProject(timesheet) ?? GetRandomProject(timesheet);

            if (project is null)
                return timesheet;

            (int day, decimal currentHours) = IncompleteDays(timesheet).First();
            project[day].WorkHours += Math.Min(1, MAX_DAILY_HOURS - currentHours);
        }

        return timesheet;
    }

    private static IEnumerable<(int day, decimal hours)> IncompleteDays(Timesheet timesheet)
    {
        return timesheet.GetBusinessDays()
            .Select((day, hours) => (day, hours: timesheet.SheetDailyHours(day)))
            .Where(day => day.hours < MAX_DAILY_HOURS);
    }

    private static Project? GetIncompleteProject(Timesheet timesheet)
    {
        var projects = timesheet.Projects.Where(p => p.WorkHoursLeft > 0).ToList();
        return projects.ElementAtOrDefault(rng.Next(projects.Count));
    }

    private static Project? GetRandomProject(Timesheet timesheet)
    {
        var projects = timesheet.Projects;
        decimal totalHours = projects.Sum(p => Math.Ceiling(p.MaxHours));
        decimal randomWeight = rng.Next(1, (int)Math.Ceiling(totalHours + 1));

        foreach (var project in projects)
        {
            randomWeight -= Math.Ceiling(project.MaxHours);
            if (randomWeight <= 0) return project;
        }

        return projects.FirstOrDefault();
    }
}
