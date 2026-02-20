using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TimesheetTracker.Core;
using TimesheetTracker.WPF.Configuration;
using TimesheetTracker.WPF.Events;
using TimesheetTracker.WPF.Models;

namespace TimesheetTracker.WPF;

public partial class MainWindowViewModel : ObservableRecipient, IRecipient<DayHoursUpdated>
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
        DailyHours = Timesheet.GetDays().ToDictionary(key => key, Timesheet.SheetDailyHours);
        IsActive = true;
    }

    [ObservableProperty]
    public partial ObservableCollection<ProjectModel> ProjectViewModels { get; set; } = [];

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
        ProjectViewModels = new(value.Projects.Select(p => new ProjectModel(p, Timesheet.DaysInMonth)));
    }

    public void Receive(DayHoursUpdated message)
    {
        DailyHours = Timesheet.GetDays().ToDictionary(key => key, Timesheet.SheetDailyHours);
    }
}
