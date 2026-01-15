using System.Linq;

namespace TimesheetTracker.Core;

/// <summary>
/// 
/// </summary>
/// <param name="day"></param>
/// <param name="hours"></param>
/// <param name="isActive">If allowed to allocate hours to the day</param>
public class Day(Project project, int day, int hours, bool isActive = true)
{
    public Project Project { get; } = project;
    public int Date { get; set; } = day;
    public int WorkHours { get; set; } = hours;
    public bool IsActive { get; set; } = isActive;
}

public class Project
{
    internal Project(
        Timesheet timesheet,
        string name,
        int maxHours,
        IEnumerable<int> workdays)
    {
        Name = name;
        MaxHours = maxHours;
        DaysInMonth = timesheet.DaysInMonth;
        WorkDays = Enumerable.Range(1, timesheet.DaysInMonth)
                      .Select(day =>
                      {
                          return new Day(this, day: day, hours: 0, isActive: workdays.Contains(day));
                      })
                      .ToList();
    }

    public Day this[int day]
    {
        get => WorkDays.First(d => d.Date == day);
        set => WorkDays.First(d => d.Date == day).WorkHours = value.WorkHours;
    }
    private List<Day> WorkDays { get; set; }
    public string Name { get; }
    public int MaxHours { get; }
    public int DaysInMonth { get; }
    public int TotalWorkedHours => WorkDays.Sum(d => d.WorkHours);
    public int WorkHoursLeft => MaxHours - TotalWorkedHours;
}

public class Timesheet(int year, int month)
{
    public int Year { get; } = year;
    public int Month { get; } = month;
    public int DaysInMonth { get; } = DateTime.DaysInMonth(year, month);
    public List<Project> Projects { get; } = [];
    public int TotalWorkedHours => Projects.Sum(p => p.TotalWorkedHours);

    public Project CreateProject(string name, int maxHours)
    {
        if (Projects.Any(p => p.Name == name))
            throw new ArgumentException("Project already exists.");

        var project = new Project(this, name, maxHours, WorkDays());
        Projects.Add(project);
        return project;
    }

    public IEnumerable<int> WorkDays()
    {
        return Enumerable.Range(1, DaysInMonth)
            .Where(day =>
            {
                var dayOfWeek = new DateTime(Year, Month, day).DayOfWeek;
                return dayOfWeek != DayOfWeek.Sunday && dayOfWeek != DayOfWeek.Saturday;
            });
    }

    public int SheetDailyHours(int day) => Projects.Sum(p => p[day].WorkHours);
}