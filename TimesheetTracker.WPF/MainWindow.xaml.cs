using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Windows;
using TimesheetTracker.Core;
using TimesheetTracker.WPF.Configuration;

namespace TimesheetTracker.WPF;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<ProjectViewModel> ProjectViewModels { get; set; } = [];

    [ObservableProperty]
    public partial Timesheet Timesheet { get; set; }

    [RelayCommand]
    private void FillSheet()
    {
        TimesheetFiller.FillTimesheet(Timesheet);
        WeakReferenceMessenger.Default.Send(new TimesheetFilled());
    }

    [RelayCommand]
    private void ShowJsonConfig()
    {
        AppConfiguration.ShowJsonConfig();
    }

    [RelayCommand]
    private void LoadProjects()
    {
        var config = AppConfiguration.GetConfig();
        if (config is null) return;

        var timesheet = new Timesheet(DateTime.Now.Year, DateTime.Now.Month);
        config.Projects.ForEach(p => timesheet.CreateProject(p.Name, p.MaxHours - (p.CurrentHours ?? 0)));

        Timesheet = timesheet;
        ProjectViewModels = new(Timesheet.Projects.Select(p => new ProjectViewModel(p)));
    }
}

public record DayHoursChanged();

public record TimesheetFilled();

public partial class DayViewModel(Day day) : ObservableObject
{
    public Day Day => day;

    [ObservableProperty]
    public partial int Hours { get; set; } = day.WorkHours;

    public bool IsActive { get; } = day.IsActive;

    partial void OnHoursChanged(int value)
    {
        int current = day.WorkHours;
        if (value == current) return;

        day.WorkHours += value - current;
        WeakReferenceMessenger.Default.Send(new DayHoursChanged());
    }

    public void Refresh()
    {
        Hours = day.WorkHours;
    }
}

public partial class ProjectViewModel :
    ObservableRecipient,
    IRecipient<DayHoursChanged>,
    IRecipient<TimesheetFilled>
{
    public Project Project { get; }
    public List<DayViewModel> Days { get; }

    public ProjectViewModel(Project project)
    {
        Project = project;
        Days = Enumerable.Range(1, project.DaysInMonth)
                         .Select(d => new DayViewModel(project[d]))
                         .ToList();

        IsActive = true;
    }

    public int TotalHours => Project.TotalWorkedHours;
    public int WorkHoursLeft => Project.WorkHoursLeft;

    public void Receive(DayHoursChanged message) => NotifyCalculations();

    public void Receive(TimesheetFilled message)
    {
        foreach (var day in Days) day.Refresh();
        NotifyCalculations();
    }

    private void NotifyCalculations()
    {
        OnPropertyChanged(nameof(TotalHours));
        OnPropertyChanged(nameof(WorkHoursLeft));
    }
}
