namespace Employees.Core.Tests.Services;

public class PairFinderTests
{
    [Fact]
    public void FindLongestPair_NoAssignments_ReturnsNull()
    {
        Assert.Null(PairFinder.FindLongestPair([]));
    }

    [Fact]
    public void FindLongestPair_SingleAssignment_ReturnsNull()
    {
        var result = PairFinder.FindLongestPair([CreateAssignment(1, 10, "2020-01-01", "2020-01-10")]);

        Assert.Null(result);
    }

    [Fact]
    public void FindLongestPair_EmployeesOnDifferentProjects_ReturnsNull()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 20, "2020-01-01", "2020-01-10"),
        ]);

        Assert.Null(result);
    }

    [Fact]
    public void FindLongestPair_SameProjectWithoutDateOverlap_ReturnsNull()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-11", "2020-01-20"),
        ]);

        Assert.Null(result);
    }

    [Fact]
    public void FindLongestPair_TwoEmployeesOverlappingOnSameProject_ReturnsOverlapInDays()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-05", "2020-01-20"),
        ]);

        AssertPair(result, 1, 2, 6);
    }

    [Fact]
    public void FindLongestPair_IdenticalPeriods_ReturnsFullPeriod()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
        ]);

        AssertPair(result, 1, 2, 10);
    }

    [Fact]
    public void FindLongestPair_OneAssignmentContainedInAnother_ReturnsInnerDuration()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-12-31"),
            CreateAssignment(2, 10, "2020-03-01", "2020-03-10"),
        ]);

        AssertPair(result, 1, 2, 10);
    }

    [Fact]
    public void FindLongestPair_AssignmentsTouchingOnSingleDay_CountsInclusiveDay()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-10", "2020-01-20"),
        ]);

        AssertPair(result, 1, 2, 1);
    }

    [Fact]
    public void FindLongestPair_SingleDayAssignments_CountsOneDay()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-05", "2020-01-05"),
            CreateAssignment(2, 10, "2020-01-05", "2020-01-05"),
        ]);

        AssertPair(result, 1, 2, 1);
    }

    [Fact]
    public void FindLongestPair_OverlapSpanningLeapDay_CountsCalendarDays()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-02-27", "2020-03-02"),
            CreateAssignment(2, 10, "2020-02-27", "2020-03-02"),
        ]);

        AssertPair(result, 1, 2, 5);
    }

    [Fact]
    public void FindLongestPair_OverlapSpanningYearBoundary_CountsCalendarDays()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-12-25", "2021-01-05"),
            CreateAssignment(2, 10, "2020-12-25", "2021-01-05"),
        ]);

        AssertPair(result, 1, 2, 12);
    }

    [Fact]
    public void FindLongestPair_EmployeeWithOverlappingAssignmentsOnSameProject_DoesNotCountSharedDaysTwice()
    {
        // Employee 1 holds two overlapping assignments on project 10, employee 2 works Jan 1-10.
        // The pair shares Jan 1-10 = 10 calendar days, no matter how many assignments cover them.
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(1, 10, "2020-01-05", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
        ]);

        AssertPair(result, 1, 2, 10);
    }

    [Fact]
    public void FindLongestPair_EmployeeWithPartiallyOverlappingAssignmentsOnSameProject_TreatsThemAsOnePeriod()
    {
        // Employee 1 is on project 10 from Jan 1 to Jan 20 (two chained assignments).
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(1, 10, "2020-01-05", "2020-01-20"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-31"),
        ]);

        AssertPair(result, 1, 2, 20);
    }

    [Fact]
    public void FindLongestPair_EmployeeWithBackToBackAssignmentsOnSameProject_CountsBothPeriods()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(1, 10, "2020-01-11", "2020-01-20"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-20"),
        ]);

        AssertPair(result, 1, 2, 20);
    }

    [Fact]
    public void FindLongestPair_EmployeeOnExtraProjectWithoutPartner_DoesNotCountExtraProjectDays()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(1, 20, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
        ]);

        AssertPair(result, 1, 2, 10);
    }

    [Fact]
    public void FindLongestPair_ThreeEmployeesOnOneProject_ReturnsPairWithLongestOverlap()
    {
        // 1&2 share Jan 1-10 (10), 1&3 share Jan 5-31 (27), 2&3 share Jan 5-10 (6).
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-31"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(3, 10, "2020-01-05", "2020-01-31"),
        ]);

        AssertPair(result, 1, 3, 27);
    }

    [Fact]
    public void FindLongestPair_SeveralAssignmentsEndingOnSameDay_ReturnsPairWithLongestOverlap()
    {
        // 1&2 share 3 days, 1&3 share 3 days, 2&3 share 6 days (Jan 5-10).
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-08", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-05", "2020-01-10"),
            CreateAssignment(3, 10, "2020-01-01", "2020-01-10"),
        ]);

        AssertPair(result, 2, 3, 6);
    }

    [Fact]
    public void FindLongestPair_MultiplePairsOnDifferentProjects_ReturnsPairWithLongestOverlap()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-05"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-05"),
            CreateAssignment(3, 20, "2020-02-01", "2020-02-28"),
            CreateAssignment(4, 20, "2020-02-01", "2020-02-28"),
        ]);

        AssertPair(result, 3, 4, 28);
    }

    [Fact]
    public void FindLongestPair_PairsWithEqualDays_ReturnsPairWithLowestEmployeeIds()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(7, 20, "2020-01-01", "2020-01-10"),
            CreateAssignment(8, 20, "2020-01-01", "2020-01-10"),
            CreateAssignment(3, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(4, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(5, 30, "2020-01-01", "2020-01-10"),
            CreateAssignment(6, 30, "2020-01-01", "2020-01-10"),
        ]);

        AssertPair(result, 3, 4, 10);
    }

    [Fact]
    public void FindLongestPair_SamePairWithTwoSeparatePeriodsOnOneProject_SumsDays()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(1, 10, "2020-06-01", "2020-06-05"),
            CreateAssignment(2, 10, "2020-06-01", "2020-06-05"),
        ]);

        AssertPair(result, 1, 2, 15);
    }

    [Fact]
    public void FindLongestPair_SamePairOnSeveralProjects_SumsDaysAcrossProjects()
    {
        // 1&2 work together 10 days on project 10 and 10 days on project 20 (20 total),
        // which beats 1&3 who share 15 days on project 30.
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(1, 20, "2020-02-01", "2020-02-10"),
            CreateAssignment(2, 20, "2020-02-01", "2020-02-10"),
            CreateAssignment(1, 30, "2020-03-01", "2020-03-15"),
            CreateAssignment(3, 30, "2020-03-01", "2020-03-15"),
        ]);

        AssertPair(result, 1, 2, 20);
    }

    [Fact]
    public void FindLongestPair_SamePairOnSeveralProjectsAtTheSameTime_CountsCalendarDaysOnce()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(1, 20, "2020-01-06", "2020-01-15"),
            CreateAssignment(2, 20, "2020-01-06", "2020-01-15"),
        ]);

        AssertPair(result, 1, 2, 15);
    }

    [Fact]
    public void FindLongestPair_InputInReverseOrder_ReturnsSameResult()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(2, 10, "2020-01-05", "2020-01-20"),
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
        ]);

        AssertPair(result, 1, 2, 6);
    }

    [Fact]
    public void FindLongestPair_EmployeeIdsInDescendingOrder_ReportsSmallerIdFirst()
    {
        var result = PairFinder.FindLongestPair(
        [
            CreateAssignment(9, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(3, 10, "2020-01-01", "2020-01-10"),
        ]);

        AssertPair(result, 3, 9, 10);
    }

    [Fact]
    public void FindLongestPair_AssignmentStartingAfterItEnds_ThrowsArgumentException()
    {
        var assignments = new[]
        {
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-20", "2020-01-05"),
        };

        Assert.Throws<ArgumentException>(() => PairFinder.FindLongestPair(assignments));
    }

    private static Assignment CreateAssignment(int employeeId, int projectId, string from, string to) =>
        new(employeeId, projectId, DateOnly.Parse(from), DateOnly.Parse(to));

    private static void AssertPair(EmployeePair? actual, int employee1, int employee2, long days)
    {
        Assert.NotNull(actual);
        Assert.Equal(employee1, actual.Employee1);
        Assert.Equal(employee2, actual.Employee2);
        Assert.Equal(days, actual.Days);
    }
}
