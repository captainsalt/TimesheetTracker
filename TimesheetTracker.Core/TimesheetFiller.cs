using System;
using System.Collections.Generic;
using System.Text;

namespace TimesheetTracker.Core;

public static class TimesheetFiller
{
    public const int MAX_DAILY_HOURS = 8;

    public static Timesheet FillTimesheet(Timesheet timesheet)
    {
        ArgumentNullException.ThrowIfNull(timesheet);

        while (IncompleteDays(timesheet).Any())
        {
            foreach (var day in IncompleteDays(timesheet))
            {
                if (RandomIncompleteProject(timesheet) is not { } project)
                    return timesheet;

                if (timesheet.ProjectTotalHours(project) == project.MaxHours)
                    return timesheet;

                timesheet.AddHoursToProject(project, day.day, 1);
            }
        }

        return timesheet;
    }

    private static IEnumerable<(int day, int hours)> IncompleteDays(Timesheet timesheet)
    {
        return Enumerable
            .Range(1, timesheet.Month.DaysInMonth)
            .Select((day) => (day, hours: timesheet.SheetDailyHours(day)))
            .Where(record => record.hours < MAX_DAILY_HOURS);
    }

    private static Project? RandomIncompleteProject(Timesheet timesheet)
    {
        var project = timesheet
            .Projects
            .Keys
            .Where(p => timesheet.ProjectTotalHours(p) < p.MaxHours)
            .FirstOrDefault();

        return project;
    }
}
