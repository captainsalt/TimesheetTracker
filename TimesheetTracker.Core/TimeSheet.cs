using System.Collections.ObjectModel;
using System.Numerics;

namespace TimesheetTracker.Core
{
    public record struct Month(DateTime Time)
    {
        public int DaysInMonth { get; } = DateTime.DaysInMonth(Time.Year, Time.Month);
    }

    public class Project(
        TimeSheet timeSheet,
        string name,
        int maxHours)
    {
        private readonly int[] _workHours = new int[timeSheet.Month.DaysInMonth];
         
        public string Name { get; } = name;
        public int MaxMonthlyHours { get; } = maxHours;

        public void AddDailyHours(int day, int hours) => _workHours[day - 1] += hours;

        public int DailyHours(int day) => _workHours[day - 1];

        public int MonthlyHours() => _workHours.Sum();
    }

    // TODO: Validate that projects don't have the same name
    public class TimeSheet(Month month)
    {
        public List<Project> Projects { get; } = [];
        public Month Month { get; } = month;

        public void AddProjects(params Project[] project)
        {
            Projects.AddRange(project);
        }

        public void RemoveProject(Project project) => Projects.Remove(project);

        /// <summary>
        /// Gets the total hours worked across all projects for a specific day.
        /// </summary>
        /// <param name="day">Day of the month</param>
        /// <returns></returns>
        public int GetDailyProjectHours(int day) => Projects.Sum(p => p.DailyHours(day));
    }
}
