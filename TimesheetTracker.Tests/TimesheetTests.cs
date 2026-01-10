using Shouldly;
using TimesheetTracker.Core;

namespace TimesheetTracker.Tests;

public class TimesheetTests
{
    private static Month _testMonth = new(new DateTime(1998, 11, 7));

    [Fact]
    public void AddHoursToProject_ShouldAddHoursToProject()
    {
        //Arrange 
        var timesheet = new Timesheet(_testMonth, offDays: []);
        var project = new Project("Project A", 10);
        timesheet.AddProjects(project);

        //Act
        timesheet.AddHoursToProject(project, day: 1, hours: 10);
        timesheet.AddHoursToProject(project, day: 2, hours: 10);

        //Assert
        timesheet.ProjectTotalHours(project).ShouldBe(20);
    }
}
