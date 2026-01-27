using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
    public MainWindowViewModel()
    {
        _ = LoadProjects();
    }

    [ObservableProperty]
    public partial ObservableCollection<ProjectViewModel> ProjectViewModels { get; set; } = [];

    [ObservableProperty]
    public partial Timesheet Timesheet { get; set; } = new Timesheet(DateTime.Now.Year, DateTime.Now.Month);

    [RelayCommand]
    private void FillSheet()
    {
        TimesheetFiller.FillTimesheet(Timesheet);
        _ = WeakReferenceMessenger.Default.Send(new TimesheetFilled());
    }

    [RelayCommand]
    private void ShowJsonConfig()
    {
        AppConfiguration.ShowJsonConfig();
    }

    [RelayCommand]
    private async Task LoadProjects()
    {
        (bool _, Config? config) = await AppConfiguration.GetConfig();
        if (config is null) return;

        var timesheet = new Timesheet(DateTime.Now.Year, DateTime.Now.Month)
        {
            ExcludedDays = config.ExcludedDays,
        };

        foreach (ProjectConfig configProject in config.Projects)
        {
            _ = timesheet.CreateProject(
                configProject.Name,
                configProject.MaxHours - (configProject.CurrentHours ?? 0m),
                configProject.DailyMinimum ?? 0m);
        }

        Timesheet = timesheet;
    }

    partial void OnTimesheetChanged(Timesheet value)
    {
        ProjectViewModels = new(value.Projects.Select(p => new ProjectViewModel(p, Timesheet.DaysInMonth)));
    }
}

public record DayHoursChanged();

public record TimesheetFilled();

public partial class DayViewModel(Day day) : ObservableObject
{
    [ObservableProperty]
    public partial decimal Hours { get; set; } = day.WorkHours;
    public Day Day => day;
    public bool IsActive { get; } = day.IsActive;

    partial void OnHoursChanged(decimal value)
    {
        var current = day.WorkHours;
        if (value == current) return;

        day.WorkHours += value - current;
        _ = WeakReferenceMessenger.Default.Send(new DayHoursChanged());
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

    public ProjectViewModel(Project project, int daysInMonth)
    {
        Project = project;
        Days = Enumerable.Range(1, daysInMonth)
                         .Select(d => new DayViewModel(project[d]))
                         .ToList();

        IsActive = true;
    }

    public decimal TotalHours => Project.TotalWorkedHours;
    public decimal WorkHoursLeft => Project.WorkHoursLeft;

    public void Receive(DayHoursChanged message)
    {
        NotifyCalculations();
    }

    public void Receive(TimesheetFilled message)
    {
        foreach (DayViewModel day in Days) day.Refresh();
        NotifyCalculations();
    }

    private void NotifyCalculations()
    {
        OnPropertyChanged(nameof(TotalHours));
        OnPropertyChanged(nameof(WorkHoursLeft));
    }
}
