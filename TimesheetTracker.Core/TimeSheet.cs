namespace TimesheetTracker.Core;

public readonly record struct Month(DateTime Time)
{
    public int DaysInMonth { get; init; } = DateTime.DaysInMonth(Time.Year, Time.Month);
}

public readonly record struct Project(string Name, int MaxHours);

public class TimeSheet(Month month, List<int> offDays)
{
    public Month Month { get; } = month;
    public List<int> OffDays { get; } = offDays;
    public Dictionary<Project, int[]> ProjectHours { get; } = [];

    public void AddProjects(params Project[] projects)
    {
        foreach (var project in projects)
            ProjectHours.TryAdd(project, new int[Month.DaysInMonth]);
    }

    public int SheetTotalHours() =>
        ProjectHours.Values.Sum(hours => hours.Sum());

    public int SheetDailyHours(int day) =>
        ProjectHours.Values.Sum(hours => hours[day - 1]);

    public int ProjectTotalHours(Project project) =>
        ProjectHours.TryGetValue(project, out var hours) ? hours.Sum() : 0;

    public int ProjectDailyHours(Project project, int day)
    {
        int[] projectHours = FetchProjectHours(project, day);
        return projectHours[day - 1];
    }

    public void AddHoursToProject(Project project, int day, int hours)
    {
        int[] projectHours = FetchProjectHours(project, day);
        projectHours[day - 1] += hours;
    }

    private int[] FetchProjectHours(Project project, int day)
    {
        if (!ProjectHours.TryGetValue(project, out var dailyHours))
            throw new ArgumentException("Project not found.");
        if (day < 1 || day > Month.DaysInMonth)
            throw new ArgumentOutOfRangeException(nameof(day));

        return dailyHours;
    }
}