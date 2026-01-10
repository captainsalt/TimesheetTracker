using System;
using System.Collections.Generic;
using System.Text;

namespace TimesheetTracker.Core;

public class TimesheetFiller
{
    const int MAX_DAILY_HOURS = 8;

    public static TimeSheet FillTimesheet(TimeSheet timesheet)
    {
        while (IncompleteWorkdays(timesheet).Any())
        {
            var (day, workedHours) = IncompleteWorkdays(timesheet).First();
            var project = RandomProject(timesheet, day);
            if (project is null)
                return timesheet;

            var hoursToAssign = Math.Min(MAX_DAILY_HOURS - workedHours, project.MaxMonthlyHours - project.MonthlyHours());
            project.AddDailyHours(day, hoursToAssign);
        }

        return timesheet;
    }

    private static IEnumerable<(int day, int workedHours)> IncompleteWorkdays(TimeSheet timesheet)
    {
        return Enumerable
            .Range(1, timesheet.Month.DaysInMonth)
            .Select((day) => (day, workedHours: timesheet.GetDailyProjectHours(day)))
            .Where(day => day.workedHours < MAX_DAILY_HOURS);
    }

    private static Project? RandomProject(TimeSheet timesheet, int day)
    {
        var projects = timesheet.Projects
          .Where(p => p.DailyHours(day) < MAX_DAILY_HOURS)
          .Where(p => p.MonthlyHours() < p.MaxMonthlyHours);

        var rng = new Random();
        var index = rng.Next(projects.Count());
        return projects.ElementAtOrDefault(index);
    }
}
