using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        var projectData = new[]
        {
            (Name: "Project A", Max: 66),
            (Name: "Project B", Max: 20),
            (Name: "Project C", Max: 10),
            (Name: "Project D", Max: 2),
            (Name: "Project E", Max: 55),
            (Name: "Project F", Max: 3)
        };

        foreach (var p in projectData)
        {
            var project = Sheet.CreateProject(p.Name, p.Max);
            var vm = new ProjectViewModel(project);

            vm.TotalsChanged += (s, e) => OnPropertyChanged(nameof(GrandTotal));
            ProjectViewModels.Add(vm);
        }
    }

    public ObservableCollection<ProjectViewModel> ProjectViewModels { get; } = [];
    public Timesheet Sheet { get; } = new(DateTime.Now.Year, DateTime.Now.Month);
    public int GrandTotal => Sheet.TotalWorkedHours;

    [RelayCommand]
    private void FillSheet()
    {
        TimesheetFiller.FillTimesheet(Sheet);
        foreach (var vm in ProjectViewModels) vm.RefreshAll();
        OnPropertyChanged(nameof(GrandTotal));
    }
}

public partial class DayViewModel(Project project, int day) : ObservableObject
{
    public event EventHandler? HoursChanged;

    [ObservableProperty]
    private int _hours = project.GetWorkedHours(day);
    
    partial void OnHoursChanged(int value)
    {
        int current = project.GetWorkedHours(day);
        project.AddWorkHours(day, value - current);
        HoursChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RefreshFromModel() => Hours = project.GetWorkedHours(day);
}

public partial class ProjectViewModel : ObservableObject
{
    public Project Project { get; }
    public List<DayViewModel> Days { get; }
    public event EventHandler? TotalsChanged;

    public ProjectViewModel(Project project)
    {
        Project = project;
        Days = Enumerable.Range(1, project.DaysInMonth)
                         .Select(d => new DayViewModel(project, d))
                         .ToList();

        foreach (var day in Days)
        {
            day.HoursChanged += (s, e) => NotifyChange();
        }
    }

    public int TotalHours => Project.TotalWorkedHours;
    public int HoursLeft => Project.WorkHoursLeft;

    private void NotifyChange()
    {
        OnPropertyChanged(nameof(TotalHours));
        OnPropertyChanged(nameof(HoursLeft));
        TotalsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RefreshAll()
    {
        foreach (var day in Days) day.RefreshFromModel();
        NotifyChange();
    }
}