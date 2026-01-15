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
    private readonly List<Day> _workDays;

    internal Project(
        Timesheet timesheet,
        string name,
        int maxHours,
        IEnumerable<int> workdays)
    {
        Timesheet = timesheet;
        Name = name;
        MaxHours = maxHours;
        _workDays = Enumerable.Range(1, timesheet.DaysInMonth)
                      .Select(day =>
                      {
                          return new Day(this, day: day, hours: 0, isActive: workdays.Contains(day));
                      })
                      .ToList();
    }

    public Day this[int day]
    {
        get => _workDays.First(d => d.Date == day);
    }
    public Timesheet Timesheet { get; }
    public string Name { get; }
    public int MaxHours { get; }
    public int TotalWorkedHours => _workDays.Sum(d => d.WorkHours);
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