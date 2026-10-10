namespace Employees.Core.Tests.Services;

public class PairFinderTests
{
    [Fact]
    public void FindLongestPair_TwoEmployeesOverlappingOnSameProject_ReturnsOverlapInDays()
    {
        var assignments = new[]
        {
            new Assignment(EmployeeId: 1, ProjectId: 10, From: new DateOnly(2020, 1, 1), To: new DateOnly(2020, 1, 10)),
            new Assignment(EmployeeId: 2, ProjectId: 10, From: new DateOnly(2020, 1, 5), To: new DateOnly(2020, 1, 20)),
        };

        var result = PairFinder.FindLongestPair(assignments);

        Assert.NotNull(result);
        Assert.Equal(1, result.Employee1);
        Assert.Equal(2, result.Employee2);
        Assert.Equal(6, result.Days);
    }

    [Fact]
    public void FindLongestPair_EmployeeWithOverlappingAssignmentsOnSameProject_DoesNotCountSharedDaysTwice()
    {
        // Employee 1 holds two overlapping assignments on project 10, employee 2 works Jan 1-10.
        // The pair shares Jan 1-10 = 10 calendar days, no matter how many assignments cover them.
        var assignments = new[]
        {
            new Assignment(EmployeeId: 1, ProjectId: 10, From: new DateOnly(2020, 1, 1), To: new DateOnly(2020, 1, 10)),
            new Assignment(EmployeeId: 1, ProjectId: 10, From: new DateOnly(2020, 1, 5), To: new DateOnly(2020, 1, 10)),
            new Assignment(EmployeeId: 2, ProjectId: 10, From: new DateOnly(2020, 1, 1), To: new DateOnly(2020, 1, 10)),
        };

        var result = PairFinder.FindLongestPair(assignments);

        Assert.NotNull(result);
        Assert.Equal(1, result.Employee1);
        Assert.Equal(2, result.Employee2);
        Assert.Equal(10, result.Days);
    }
}
