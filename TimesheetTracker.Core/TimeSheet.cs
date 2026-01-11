namespace TimesheetTracker.Core;

public class Project
{
    private readonly int[] _workHours;

    internal Project(string name, int maxHours, int daysInMonth)
    {
        Name = name;
        MaxHours = maxHours;
        DaysInMonth = daysInMonth;
        _workHours = new int[daysInMonth];
    }

    public string Name { get; }
    public int MaxHours { get; }
    public int DaysInMonth { get; }

    public int TotalWorkedHours => _workHours.Sum();
    public int WorkHoursLeft => MaxHours - TotalWorkedHours;

    public int GetWorkedHours(int day) => _workHours[day - 1];

    public int AddWorkHours(int day, int hours) => _workHours[day - 1] += hours;
};

public class Timesheet(int year, int month)
{
    public int DaysInMonth { get; } = DateTime.DaysInMonth(year, month);

    public List<Project> Projects { get; } = [];

    public int TotalWorkedHours => Projects.Sum(p => p.TotalWorkedHours);

    public Project CreateProject(string name, int maxHours)
    {
        if (Projects.Any(p => p.Name == name))
            throw new ArgumentException("Project already exists.");

        var project = new Project(name, maxHours, DaysInMonth);
        Projects.Add(project);
        return project;
    }

    public int SheetDailyHours(int day) => Projects.Sum(p => p.GetWorkedHours(day));
}