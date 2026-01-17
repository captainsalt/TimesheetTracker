using Shouldly;
using TimesheetTracker.Core;

namespace TimesheetTracker.Tests;

public class TimesheetFillerTests
{
    private static int MaxMonthlyHours(Timesheet timesheet) =>
        timesheet.GetBusinessDays().Count() * TimesheetFiller.MAX_DAILY_HOURS;

    [Fact]
    public void FillTimesheet_ShouldFillTimesheet()
    {
        // Arrange
        var timesheet = new Timesheet(1998, 7);
        timesheet.CreateProject("Project A", MaxMonthlyHours(timesheet));

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
        var project = timesheet.CreateProject("Project A", 300);

        //Act 
        TimesheetFiller.FillTimesheet(timesheet);

        //Assert
        project.TotalWorkedHours.ShouldBe(MaxMonthlyHours(timesheet));
    }

    [Fact]
    public void FillTimesheet_UsesMultipleProjects()
    {
        // Arrange
        var timesheet = new Timesheet(1998, 7);
        var projectA = timesheet.CreateProject("Project A", 10);
        var projectB = timesheet.CreateProject("Project B", 10);

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
        var projectData = new[]
        {
            (Name: "Project A", Max: 66),
            (Name: "Project B", Max: 20),
            (Name: "Project C", Max: 10),
            (Name: "Project D", Max: 2),
            (Name: "Project E", Max: 55),
            (Name: "Project F", Max: 3)
        };

        foreach (var (Name, Max) in projectData) timesheet.CreateProject(Name, Max);

        //Act 
        TimesheetFiller.FillTimesheet(timesheet);

        //Assert
        timesheet.SheetDailyHours(1).ShouldBe(TimesheetFiller.MAX_DAILY_HOURS);
    }
}
