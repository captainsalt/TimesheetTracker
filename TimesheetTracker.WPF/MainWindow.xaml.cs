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

    public ObservableCollection<ProjectViewModel> ProjectViewModels { get; set; } = [];

    [ObservableProperty]
    public partial Timesheet Timesheet { get; set; }
}

public record DayHoursChanged();

public partial class DayViewModel(Project project, int day) : ObservableObject
{
    public int Day => day;

    [ObservableProperty]
    public partial int Hours { get; set; }

    partial void OnHoursChanged(int value)
    {
        int current = project.GetWorkedHours(Day);
        project.AddWorkHours(Day, value - current);
        WeakReferenceMessenger.Default.Send(new DayHoursChanged());
    }
}

public partial class ProjectViewModel : ObservableObject, IRecipient<DayHoursChanged>
{
    public ProjectViewModel(Project project)
    {
        Project = project;
        Days = Enumerable.Range(1, project.DaysInMonth).Select(day => new DayViewModel(project, day)).ToList();
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public Project Project { get; }

    public int TotalHours => Project.TotalWorkedHours;

    public int WorkHoursLeft => Project.WorkHoursLeft;

    public List<DayViewModel> Days { get; }

    public void Receive(DayHoursChanged message)
    {
        OnPropertyChanged(nameof(TotalHours));
        OnPropertyChanged(nameof(WorkHoursLeft));
    }
}
