using System;
using System.Collections.Generic;
using System.Text;
using TimesheetTracker.Core;

namespace TimesheetTracker.Tests;

public class TimesheetFillerTests : IDisposable
{
    private static Month _testMonth = new(new DateTime(1998, 11, 7));
    private static TimeSheet _timeSheet = new(_testMonth);
    private static List<Project> _testProjects =
    [
        new Project(_timeSheet, "Project A", maxHours: 66),
        new Project(_timeSheet, "Project B", maxHours: 20),
        new Project(_timeSheet, "Project C", maxHours: 10),
        new Project(_timeSheet, "Project D", maxHours: 2),
        new Project(_timeSheet, "Project E", maxHours: 55),
        new Project(_timeSheet, "Project F", maxHours: 3),
    ];

    public TimesheetFillerTests() => _timeSheet = new(_testMonth);

    public void Dispose() => _timeSheet = new(_testMonth);

    [Fact]
    public void FillTimesheet_ShouldReturnTimesheet()
    {
        // Arrange
        var timesheet = new TimeSheet(_testMonth);

        // Act
        var result = TimesheetFiller.FillTimesheet(timesheet);

        // Assert
        Assert.Equal(timesheet, result);
        Assert.True(result.Projects.All(p => p.MonthlyHours() == p.MaxMonthlyHours));
    }
}
