namespace TimesheetTracker.Core;

public static class TimesheetFiller
{
    public const int MAX_DAILY_HOURS = 8;

    public static Timesheet FillTimesheet(Timesheet timesheet)
    {
        ArgumentNullException.ThrowIfNull(timesheet);

        while (IncompleteDays(timesheet).Any())
        {
            var day = IncompleteDays(timesheet).First();

            if (GetIncompleteProject(timesheet) is not { } project)
                return timesheet;

            project.AddWorkHours(day.day, 1);
        }

        return timesheet;
    }

    private static IEnumerable<(int day, int hours)> IncompleteDays(Timesheet timesheet)
    {
        return Enumerable.Range(1, timesheet.DaysInMonth)
            .Select((day, hours) => (day, hours: timesheet.SheetDailyHours(day)))
            .Where(day => day.hours < MAX_DAILY_HOURS);
    }

    private static Project? GetIncompleteProject(Timesheet timesheet)
    {
        return timesheet.Projects
            .Where(p => p.WorkHoursLeft > 0)
            .OrderBy(p => p.WorkHoursLeft)
            .FirstOrDefault();
    }
}
