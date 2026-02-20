using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using TimesheetTracker.Core;
using TimesheetTracker.WPF.Events;

namespace TimesheetTracker.WPF;

public partial class DayModel(Day day) : ObservableObject
{
    [ObservableProperty]
    public partial decimal Hours { get; set; } = day.WorkHours;

    public Day Day => day;

    public bool IsActive { get; } = day.IsActive;

    partial void OnHoursChanged(decimal value)
    {
        var current = Day.WorkHours;
        if (value == current) return;

        Day.WorkHours += value - current;
        _ = WeakReferenceMessenger.Default.Send(new DayHoursChanged(Day));
    }

    public void Refresh()
    {
        Hours = Day.WorkHours;
    }
}
