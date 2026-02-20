using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using TimesheetTracker.Core;
using TimesheetTracker.WPF.Events;

namespace TimesheetTracker.WPF.Models;

public partial class ProjectModel :
    ObservableRecipient,
    IRecipient<DayHoursUpdated>,
    IRecipient<TimesheetFilled>
{
    public Project Project { get; }
    public List<DayModel> Days { get; }

    public ProjectModel(Project project, int daysInMonth)
    {
        Project = project;
        Days = Enumerable.Range(1, daysInMonth)
                         .Select(d => new DayModel(project[d]))
                         .ToList();

        IsActive = true;
    }

    public string Name => Project.Name;

    public decimal TotalWorkedHours => Project.TotalWorkedHours;

    public decimal WorkHoursLeft => Project.WorkHoursLeft;

    public decimal MaxHours => Project.MaxHours;

    private void NotifyCalculations()
    {
        OnPropertyChanged(nameof(TotalWorkedHours));
        OnPropertyChanged(nameof(WorkHoursLeft));
    }

    public void Receive(DayHoursUpdated message)
    {
        NotifyCalculations();
    }

    public void Receive(TimesheetFilled message)
    {
        foreach (DayModel day in Days) day.Refresh();
        NotifyCalculations();
    }
}
