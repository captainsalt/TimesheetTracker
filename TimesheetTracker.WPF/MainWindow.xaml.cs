using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
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

public partial class MainWindowViewModel : ObservableRecipient, IRecipient<DayHoursChanged>
{
    [GeneratedRegex(@"timesheet_(?<year>\d+)_(?<month>\d+)\.json$")]
    private static partial Regex TimesheetRegex();

    public MainWindowViewModel()
    {
        _ = Initialize();
    }

    public async Task Initialize()
    {
        await LoadTimesheet(DateTime.Now.Year, DateTime.Now.Month);
        DailyHours = Timesheet.GetDays().ToDictionary(key => key, element => Timesheet.SheetDailyHours(element));
        IsActive = true;
    }

    [ObservableProperty]
    public partial ObservableCollection<ProjectViewModel> ProjectViewModels { get; set; } = [];

    [ObservableProperty]
    public partial Timesheet Timesheet { get; set; } = new Timesheet(DateTime.Now.Year, DateTime.Now.Month);

    [ObservableProperty]
    public partial Dictionary<int, decimal> DailyHours { get; set; } = [];

    [RelayCommand]
    private void FillSheet()
    {
        TimesheetFiller.FillTimesheet(Timesheet);
        _ = WeakReferenceMessenger.Default.Send(new TimesheetFilled());
    }

    [RelayCommand]
    private void OpenTimesheetDirectory()
    {
        AppConfiguration.OpenTimesheetDirectory();
    }

    [RelayCommand]
    private async Task LoadTimesheet()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            DefaultExt = ".json",
            Filter = "JSON files (.json)|*.json|All files (*.*)|*.*"
        };

        bool? result = openFileDialog.ShowDialog();

        if (result is false or null)
        {
            return;
        }

        GroupCollection matchCollection = TimesheetRegex().Match(openFileDialog.FileName).Groups;
        int year = int.Parse(matchCollection["year"].Value);
        int month = int.Parse(matchCollection["month"].Value);
        await LoadTimesheet(year, month);
    }

    private async Task LoadTimesheet(int year, int month)
    {
        (_, Timesheet? timesheet) = await AppConfiguration.LoadTimesheet(year, month);

        if (timesheet is null)
        {
            _ = MessageBox.Show("Could not load configuration timesheet", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Timesheet = timesheet;
    }

    [RelayCommand]
    private async Task SaveTimesheet()
    {
        await AppConfiguration.SaveTimesheet(Timesheet);
    }

    partial void OnTimesheetChanged(Timesheet value)
    {
        ProjectViewModels = new(value.Projects.Select(p => new ProjectViewModel(p, Timesheet.DaysInMonth)));
    }

    public void Receive(DayHoursChanged message)
    {
        DailyHours = Timesheet.GetDays().ToDictionary(key => key, element => Timesheet.SheetDailyHours(element));
    }
}

public record DayHoursChanged(Day Day);

public record TimesheetFilled();

public partial class DayViewModel(Day day) : ObservableObject
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

    public string Name => Project.Name;

    public decimal TotalWorkedHours => Project.TotalWorkedHours;

    public decimal WorkHoursLeft => Project.WorkHoursLeft;

    public decimal MaxHours => Project.MaxHours;

    private void NotifyCalculations()
    {
        OnPropertyChanged(nameof(TotalWorkedHours));
        OnPropertyChanged(nameof(WorkHoursLeft));
    }

    public void Receive(DayHoursChanged message)
    {
        NotifyCalculations();
    }

    public void Receive(TimesheetFilled message)
    {
        foreach (DayViewModel day in Days) day.Refresh();
        NotifyCalculations();
    }
}
