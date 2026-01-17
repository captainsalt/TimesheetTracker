
namespace TimesheetTracker.Core;

public static class TimesheetFiller
{
    public const int MAX_DAILY_HOURS = 8;
    public static readonly Random rng = new();

    public static void FillTimesheet(Timesheet timesheet)
    {
        ArgumentNullException.ThrowIfNull(timesheet);

        foreach (Project? project in timesheet.Projects.Where(p => p.DailyMinimum > 0))
        {
            foreach (Day? day in project.Where(d => d.IsActive))
            {
                if (day.WorkHours < project.DailyMinimum)
                    day.WorkHours += project.DailyMinimum;
            }
        }

        while (IncompleteDays(timesheet).FirstOrDefault() is { day: > 0 } incompleteDay)
        {
            Project? project = GetIncompleteProject(timesheet) ?? GetRandomProject(timesheet);

            if (project is null)
                return;

            (int day, decimal currentHours) = incompleteDay;
            project[day].WorkHours += Math.Min(1, MAX_DAILY_HOURS - currentHours);
        }
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
        List<Project> projects = timesheet.Projects;
        decimal totalHours = projects.Sum(p => Math.Ceiling(p.MaxHours));
        decimal randomWeight = rng.Next(1, (int)Math.Ceiling(totalHours + 1));

        foreach (Project project in projects)
        {
            randomWeight -= Math.Ceiling(project.MaxHours);
            if (randomWeight <= 0) return project;
        }

        return projects.FirstOrDefault();
    }
}
