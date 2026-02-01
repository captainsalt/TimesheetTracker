using System.Collections;

namespace TimesheetTracker.Core;

/// <summary>
/// 
/// </summary>
/// <param name="day"></param>
/// <param name="hours"></param>
/// <param name="isActive">If allowed to allocate hours to the day</param>
public class Day(int day, int hours, bool isActive = true)
{
    public int Date { get; init; } = day;
    public decimal WorkHours { get; set; } = hours;
    public bool IsActive { get; set; } = isActive;
}

public class Project
{
    private readonly Dictionary<int, Day> _workDays;

    internal Project(
        int daysInMonth,
        string name,
        decimal maxHours,
        decimal dailyMinimum,
        IEnumerable<int> businessDays)
    {
        Name = name;
        MaxHours = maxHours;
        DailyMinimum = dailyMinimum;
        _workDays = Enumerable.Range(1, daysInMonth)
            .ToDictionary(
                day => day,
                day => new Day(day, 0, businessDays.Contains(day))
            );
    }

    public Day this[int day] => _workDays[day];
    public IReadOnlyDictionary<int, Day> WorkDays => _workDays;
    public string Name { get; init; }
    public decimal MaxHours { get; init; }
    public decimal DailyMinimum { get; init; }
    public decimal TotalWorkedHours => _workDays.Values.Sum(d => d.WorkHours);
    public decimal WorkHoursLeft => MaxHours - TotalWorkedHours;
}

public class Timesheet(int year, int month)
{
    public int Year { get; init; } = year;
    public int Month { get; init; } = month;
    public int DaysInMonth => DateTime.DaysInMonth(Year, Month); 
    public List<Project> Projects { get; init; } = [];
    public List<int> ExcludedDays { get; init; } = [];
    public decimal TotalWorkedHours => Projects.Sum(p => p.TotalWorkedHours);

    public Project CreateProject(string name, decimal maxHours, decimal dailyMinimum = 0)
    {
        if (Projects.Any(p => p.Name == name))
            throw new ArgumentException("Project already exists.");

        var project = new Project(DaysInMonth, name, maxHours, dailyMinimum, GetBusinessDays());
        Projects.Add(project);
        return project;
    }

    public IEnumerable<int> GetBusinessDays()
    {
        return Enumerable.Range(1, DaysInMonth)
            .Where(day => ExcludedDays.Contains(day) == false)
            .Where(day =>
            {
                DayOfWeek dayOfWeek = new DateTime(Year, Month, day).DayOfWeek;
                return dayOfWeek is not DayOfWeek.Sunday and not DayOfWeek.Saturday;
            });
    }

    public decimal SheetDailyHours(int day)
    {
        return Projects.Sum(p => p[day].WorkHours);
    }
}