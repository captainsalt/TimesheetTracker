using System.Text.Json.Serialization;

namespace TimesheetTracker.Core;

public class Day
{
    public Day() { }
    public Day(int day, decimal hours, bool isActive = true)
    {
        Date = day;
        WorkHours = hours;
        IsActive = isActive;
    }

    public int Date { get; set; }

    public decimal WorkHours { get; set; }

    public bool IsActive { get; set; }
}

public class Project
{
    public Project() { }

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
        WorkDays = Enumerable.Range(1, daysInMonth)
            .ToDictionary(
                day => day,
                day => new Day(day, 0, businessDays.Contains(day))
            );
    }

    [JsonIgnore]
    public Day this[int day] => WorkDays[day];

    public string Name { get; set; } = string.Empty;

    public decimal MaxHours { get; set; }

    public decimal DailyMinimum { get; set; }

    public Dictionary<int, Day> WorkDays { get; set; } = [];

    [JsonIgnore]
    public decimal TotalWorkedHours => WorkDays.Values.Sum(d => d.WorkHours);

    [JsonIgnore]
    public decimal WorkHoursLeft => MaxHours - TotalWorkedHours;
}

public class Timesheet
{
    public Timesheet() { }
    public Timesheet(int year, int month)
    {
        Year = year;
        Month = month;
    }

    public int Year { get; set; }

    public int Month { get; set; }

    public List<Project> Projects { get; set; } = [];

    public List<int> ExcludedDays { get; set; } = [];

    [JsonIgnore]
    public int DaysInMonth => DateTime.DaysInMonth(Year, Month);

    [JsonIgnore]
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
            .Where(day => !ExcludedDays.Contains(day))
            .Where(day => new DateTime(Year, Month, day).DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday);
    }

    public decimal SheetDailyHours(int day)
    {
        return Projects.Sum(project => project[day].WorkHours);
    }
}