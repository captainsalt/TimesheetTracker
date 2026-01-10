using Shouldly;
using TimesheetTracker.Core;

namespace TimesheetTracker.Tests;

public class TimesheetFillerTests
{
    private static int MaxMonthlyHours(Timesheet timesheet) =>
        timesheet.DaysInMonth * TimesheetFiller.MAX_DAILY_HOURS;

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
        projectA.TotalWorkedHours.ShouldBe(10);
        projectB.TotalWorkedHours.ShouldBe(10);
    }
}
