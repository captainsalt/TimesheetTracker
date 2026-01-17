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

            (int day, int _) = IncompleteDays(timesheet).First();
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
        var projects = timesheet.Projects.Where(p => p.WorkHoursLeft > 0).ToList();
        return projects.ElementAtOrDefault(rng.Next(projects.Count));
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
