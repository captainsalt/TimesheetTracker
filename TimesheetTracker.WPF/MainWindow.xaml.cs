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
public partial class MainWindow : Window, INotifyPropertyChanged
{
    public ObservableCollection<ProjectViewModel> ProjectViewModels { get; } = [];
    public Timesheet Sheet { get; } = new(1997, 7);

    public int GrandTotal => Sheet.TotalWorkedHours;

    public MainWindow()
    {
        InitializeComponent();

        var projectData = new[]
        {
            (Name: "Project X", Max: 1000), 
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
            ProjectViewModels.Add(new ProjectViewModel(project, () => OnPropertyChanged(nameof(GrandTotal))));
        }

        DataContext = this;
    }

    [RelayCommand]
    private void FillSheet()
    {
        TimesheetFiller.FillTimesheet(Sheet);
        foreach (var vm in ProjectViewModels) vm.RefreshAll();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public partial class DayViewModel : ObservableObject
{
    private readonly Project _project;
    private readonly int _day;
    private readonly Action _onChanged;

    public DayViewModel(Project project, int day, Action onChanged)
    {
        _project = project;
        _day = day;
        _onChanged = onChanged;
        _hours = project.GetWorkedHours(day);
    }

    [ObservableProperty]
    private int _hours;

    partial void OnHoursChanged(int value)
    {
        int current = _project.GetWorkedHours(_day);
        _project.AddWorkHours(_day, value - current);
        _onChanged?.Invoke();
    }

    public void RefreshFromModel() => Hours = _project.GetWorkedHours(_day);
}

public partial class ProjectViewModel : ObservableObject
{
    private readonly Action _onParentChanged;
    public Project Model { get; }
    public List<DayViewModel> Days { get; }

    public ProjectViewModel(Project project, Action onParentChanged)
    {
        Model = project;
        _onParentChanged = onParentChanged;
        Days = Enumerable.Range(1, project.TotalWorkedHours + project.WorkHoursLeft == 0 ? 31 : 31) // Using 31 or project logic
            .Select(d => new DayViewModel(project, d, NotifyChange)).ToList();
    }

    public int TotalHours => Model.TotalWorkedHours;
    public int HoursLeft => Model.WorkHoursLeft;

    private void NotifyChange()
    {
        OnPropertyChanged(nameof(TotalHours));
        OnPropertyChanged(nameof(HoursLeft));
        _onParentChanged?.Invoke();
    }

    public void RefreshAll()
    {
        foreach (var day in Days) day.RefreshFromModel();
        NotifyChange();
    }
}