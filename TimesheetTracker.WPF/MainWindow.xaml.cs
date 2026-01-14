using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimesheetTracker.Core;
using TimesheetTracker.WPF.Services;

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
        Timesheet = new(DateTime.Now.Year, DateTime.Now.Month);

        new List<(string name, int maxHours)>
        {
            (name: "Project A", maxHours: 66),
            (name: "Project B", maxHours: 20),
            (name: "Project C", maxHours: 10),
            (name: "Project D", maxHours: 2),
            (name: "Project E", maxHours: 55),
            (name: "Project F", maxHours: 3)
        }.ForEach(p => Timesheet.CreateProject(p.name, p.maxHours));
        ProjectViewModels = new(Timesheet.Projects.Select(p => new ProjectViewModel(p)));
    }

    [ObservableProperty]
    public partial ObservableCollection<ProjectViewModel> ProjectViewModels { get; set; } = [];

    [ObservableProperty]
    public partial Timesheet Timesheet { get; set; }

    [RelayCommand]
    void FillSheet()
    {
        TimesheetFiller.FillTimesheet(Timesheet);
        WeakReferenceMessenger.Default.Send(new TimesheetFilled());
    }

    [RelayCommand]
    void ShowJsonConfig()
    {
        AppConfiguration.ShowJsonConfig();
    }

    [RelayCommand]
    void LoadProjects()
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

public partial class DayViewModel(Project project, int day) : ObservableObject
{
    public int Day => day;

    [ObservableProperty]
    public partial int Hours { get; set; } = project.GetWorkedHours(day);

    partial void OnHoursChanged(int value)
    {
        int current = project.GetWorkedHours(Day);
        if (value == current) return;

        project.AddWorkHours(Day, value - current);
        WeakReferenceMessenger.Default.Send(new DayHoursChanged());
    }

    public void Refresh()
    {
        Hours = project.GetWorkedHours(day);
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
                         .Select(d => new DayViewModel(project, d))
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
