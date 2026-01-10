using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;
using TimesheetTracker.Core;

namespace TimesheetTracker.Tests;

public class TimesheetFillerTests
{
    private static Month _testMonth = new(new DateTime(1998, 11, 7));

    [Fact]
    public void TimesheetFiller_ShouldFillTimesheet()
    {
        //Arrange 
        var timesheet = new Timesheet(_testMonth, offDays: []);
        var project = new Project("Project A", _testMonth.DaysInMonth * 8);
        timesheet.AddProjects(project);

        //Act 
        TimesheetFiller.FillTimesheet(timesheet);

        //Assert
        timesheet.ProjectTotalHours(project).ShouldBe(_testMonth.DaysInMonth * 8);
    }

    [Fact]
    public void TimesheetFiller_ShouldNotOverFill()
    {
        //Arrange 
        var timesheet = new Timesheet(_testMonth, offDays: []);
        var project = new Project("Project A", 1_000);
        timesheet.AddProjects(project);

        //Act 
        TimesheetFiller.FillTimesheet(timesheet);

        //Assert
        timesheet.ProjectTotalHours(project).ShouldBe(_testMonth.DaysInMonth * 8);
    }

    [Fact]
    public void TimesheetFiller_UsesAllProjects()
    {
        //Arrange 
        var timesheet = new Timesheet(_testMonth, offDays: []);
        var projectA = new Project("Project A", 10);
        var projectB = new Project("Project B", 10);
        var projectC = new Project("Project C", 10);
        var projectD = new Project("Project D", 1);
        timesheet.AddProjects(projectA, projectB, projectC, projectD);

        //Act 
        TimesheetFiller.FillTimesheet(timesheet);

        //Assert
        timesheet.ProjectTotalHours(projectA).ShouldBe(10);
        timesheet.ProjectTotalHours(projectB).ShouldBe(10);
        timesheet.ProjectTotalHours(projectC).ShouldBe(10);
        timesheet.ProjectTotalHours(projectD).ShouldBe(1);
    }
}
