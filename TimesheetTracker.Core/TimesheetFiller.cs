namespace TimesheetTracker.Core;

public static class TimesheetFiller
{
    public const int MAX_DAILY_HOURS = 8;
    public static Random rng = new();

    public static Timesheet FillTimesheet(Timesheet timesheet)
    {
        ArgumentNullException.ThrowIfNull(timesheet);

        while (IncompleteDays(timesheet).Any())
        {
            (int day, int _) = IncompleteDays(timesheet).First();

            if (GetIncompleteProject(timesheet) is not { } project)
                return timesheet;

            project[day].WorkHours += 1;
        }

        return timesheet;
    }

    private static IEnumerable<(int day, int hours)> IncompleteDays(Timesheet timesheet)
    {
        return timesheet.GetBusinessDays()
            .Select((day, hours) => (day, hours: timesheet.SheetDailyHours(day)))
            .Where(day => day.hours < MAX_DAILY_HOURS);
    }

    private static Project? GetIncompleteProject(Timesheet timesheet)
    {
        return timesheet.Projects
            .Where(p => p.WorkHoursLeft > 0)
            .OrderBy(p => p.WorkHoursLeft)
            .FirstOrDefault() ?? GetRandomProject(timesheet);
    }

    private static Project? GetRandomProject(Timesheet timesheet)
    {
        var projects = timesheet.Projects;
        int totalHours = projects.Sum(p => p.MaxHours);
        int randomWeight = rng.Next(1, totalHours + 1);

        foreach (var project in projects)
        {
            randomWeight -= project.MaxHours;
            if (randomWeight <= 0) return project;
        }

        return projects.FirstOrDefault();
    }
}
