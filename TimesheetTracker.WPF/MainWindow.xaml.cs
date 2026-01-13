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
    private Timesheet _timesheet;

    public MainWindowViewModel()
    {
        _timesheet = new(DateTime.Now.Year, DateTime.Now.Month);

        new List<(string name, int maxHours)>
        {
            ("Project A", 90),
            ("Project B", 80),
            ("Project C", 70),
        }.ForEach(p => _timesheet.CreateProject(p.name, p.maxHours));

        ProjectViewModels = [.. _timesheet.Projects.Select(p => new ProjectViewModel(p))];
    }

    public ObservableCollection<ProjectViewModel> ProjectViewModels { get; set; } = [];
}

public record HoursChanged();

public partial class DayViewModel(Project project, int day) : ObservableObject
{
    public event EventHandler? HoursChanged;

    [ObservableProperty]
    private int _hours = project.GetWorkedHours(day);

    public int Day { get; } = day;

    partial void OnHoursChanged(int value)
    {
        int current = project.GetWorkedHours(Day);
        project.AddWorkHours(Day, value - current);
        WeakReferenceMessenger.Default.Send(new HoursChanged());
    }
}

public partial class ProjectViewModel : ObservableObject
{
    public ProjectViewModel(Project project)
    {
        Project = project;
        Days = [.. Enumerable.Range(1, project.DaysInMonth).Select(day => new DayViewModel(project, day))];

        WeakReferenceMessenger.Default.Register<HoursChanged>(this, (r, m) =>
        {
            OnDayUpdate();
        });
    }

    public Project Project { get; }

    public int TotalHours => Project.TotalWorkedHours;

    public int WorkHoursLeft => Project.WorkHoursLeft;

    public ObservableCollection<DayViewModel> Days { get; set; }

    void OnDayUpdate()
    {
        OnPropertyChanged(nameof(TotalHours));
        OnPropertyChanged(nameof(WorkHoursLeft));
    }
}
