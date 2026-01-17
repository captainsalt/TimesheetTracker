using Shouldly;
using TimesheetTracker.Core;

namespace TimesheetTracker.Tests;

public class TimesheetFillerTests
{
    private static int MaxMonthlyHours(Timesheet timesheet)
    {
        return timesheet.GetBusinessDays().Count() * TimesheetFiller.MAX_DAILY_HOURS;
    }

    [Fact]
    public void FillTimesheet_ShouldFillTimesheet()
    {
        // Arrange
        var timesheet = new Timesheet(1998, 7);
        _ = timesheet.CreateProject("Project A", MaxMonthlyHours(timesheet));

        //Act 
        TimesheetFiller.FillTimesheet(timesheet);

        //Assert
        timesheet.TotalWorkedHours.ShouldBe(MaxMonthlyHours(timesheet));
    }

    [Fact]
    public void FillTimesheet_ShouldNotOverfillTimesheet()
    {
        // Arrange
        var timesheet = new Timesheet(1998, 7);
        _ = timesheet.CreateProject("Project A", MaxMonthlyHours(timesheet) * 10);
        _ = timesheet.CreateProject("Project B", MaxMonthlyHours(timesheet) * 10, dailyMinimum: 2);

        //Act 
        TimesheetFiller.FillTimesheet(timesheet);
        TimesheetFiller.FillTimesheet(timesheet);
        TimesheetFiller.FillTimesheet(timesheet);

        //Assert
        timesheet.TotalWorkedHours.ShouldBe(MaxMonthlyHours(timesheet));
    }

    [Fact]
    public void FillTimesheet_UsesMultipleProjects()
    {
        // Arrange
        var timesheet = new Timesheet(1998, 7);
        Project projectA = timesheet.CreateProject("Project A", 10);
        Project projectB = timesheet.CreateProject("Project B", 10);

        //Act 
        TimesheetFiller.FillTimesheet(timesheet);

        //Assert
        projectA.TotalWorkedHours.ShouldBeGreaterThan(0);
        projectB.TotalWorkedHours.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void FillTimesheet_ShouldFillFromSmallestDays()
    {
        // Arrange
        var timesheet = new Timesheet(1998, 7);
        (string Name, int Max)[] projectData = new[]
        {
            (Name: "Project A", Max: 66),
            (Name: "Project B", Max: 20),
            (Name: "Project C", Max: 10),
            (Name: "Project D", Max: 2),
            (Name: "Project E", Max: 55),
            (Name: "Project F", Max: 3)
        };

        foreach ((string? Name, int Max) in projectData) _ = timesheet.CreateProject(Name, Max);

        //Act 
        TimesheetFiller.FillTimesheet(timesheet);

        //Assert
        timesheet.SheetDailyHours(1).ShouldBe(TimesheetFiller.MAX_DAILY_HOURS);
    }
}
