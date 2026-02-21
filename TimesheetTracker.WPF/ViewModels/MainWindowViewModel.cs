using System.Collections.ObjectModel;
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
    public MainWindowViewModel()
    {
        _ = Initialize();
    }

    public async Task Initialize()
    {
        (_, Timesheet? timesheet) = await AppConfiguration.LoadTimesheet(DateTime.Now.Year, DateTime.Now.Month);

        if (timesheet is null)
        {
            _ = MessageBox.Show("Could not load timesheet for current month, creating new timesheet", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        Timesheet = timesheet ?? new Timesheet(DateTime.Now.Year, DateTime.Now.Month);
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

        (Exception? exception, Timesheet? timesheet) = await AppConfiguration.LoadTimesheet(openFileDialog.FileName);

        if (exception is { } ex)
        {
            _ = MessageBox.Show($"Could not load configuration timesheet: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Timesheet = timesheet switch
        {
            null => Timesheet,
            _ => timesheet
        };
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
