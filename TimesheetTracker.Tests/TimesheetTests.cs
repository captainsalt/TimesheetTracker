using TimesheetTracker.Core;

namespace TimesheetTracker.Tests;

public class TimesheetTests
{
    private static Month _testMonth = new(new DateTime(1998, 11, 7));

    [Fact]
    public void GetDailyProjectHours_ShouldCalculateDailyHours()
    {
        var project1 = new Project(_testMonth, "Project 1", maxHours: 0);
        var project2 = new Project(_testMonth, "Project 2", maxHours: 0);

        project1.AddDailyHours(day: 1, hours: 2); project1.AddDailyHours(day: 2, hours: 4);
        project2.AddDailyHours(day: 1, hours: 2); project2.AddDailyHours(day: 2, hours: 4);

        var manager = new TimeSheet([project1, project2], _testMonth);

        Assert.Equal(4, manager.GetDailyProjectHours(1));
        Assert.Equal(8, manager.GetDailyProjectHours(2));
    }
}
