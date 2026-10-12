namespace Employees.Core.Tests.Services;

public class PairFinderTests
{
    private static readonly PairFinder Finder = new();

    [Fact]
    public void FindLongestPairs_NoAssignments_ReturnsEmptyList()
    {
        Assert.Empty(Finder.FindLongestPairs([]));
    }

    [Fact]
    public void FindLongestPairs_SingleAssignment_ReturnsEmptyList()
    {
        var result = Finder.FindLongestPairs([CreateAssignment(1, 10, "2020-01-01", "2020-01-10")]);

        Assert.Empty(result);
    }

    [Fact]
    public void FindLongestPairs_EmployeesOnDifferentProjects_ReturnsEmptyList()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 20, "2020-01-01", "2020-01-10"),
        ]);

        Assert.Empty(result);
    }

    [Fact]
    public void FindLongestPairs_SameProjectWithoutDateOverlap_ReturnsEmptyList()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-11", "2020-01-20"),
        ]);

        Assert.Empty(result);
    }

    [Fact]
    public void FindLongestPairs_TwoEmployeesOverlappingOnSameProject_ReturnsOverlapInDays()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-05", "2020-01-20"),
        ]);

        AssertPair(result, 1, 2, (10, 6));
    }

    [Fact]
    public void FindLongestPairs_IdenticalPeriods_ReturnsFullPeriod()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
        ]);

        AssertPair(result, 1, 2, (10, 10));
    }

    [Fact]
    public void FindLongestPairs_OneAssignmentContainedInAnother_ReturnsInnerDuration()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-12-31"),
            CreateAssignment(2, 10, "2020-03-01", "2020-03-10"),
        ]);

        AssertPair(result, 1, 2, (10, 10));
    }

    [Fact]
    public void FindLongestPairs_AssignmentsTouchingOnSingleDay_CountsInclusiveDay()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-10", "2020-01-20"),
        ]);

        AssertPair(result, 1, 2, (10, 1));
    }

    [Fact]
    public void FindLongestPairs_SingleDayAssignments_CountsOneDay()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-05", "2020-01-05"),
            CreateAssignment(2, 10, "2020-01-05", "2020-01-05"),
        ]);

        AssertPair(result, 1, 2, (10, 1));
    }

    [Fact]
    public void FindLongestPairs_OverlapSpanningLeapDay_CountsCalendarDays()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-02-27", "2020-03-02"),
            CreateAssignment(2, 10, "2020-02-27", "2020-03-02"),
        ]);

        AssertPair(result, 1, 2, (10, 5));
    }

    [Fact]
    public void FindLongestPairs_OverlapSpanningYearBoundary_CountsCalendarDays()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-12-25", "2021-01-05"),
            CreateAssignment(2, 10, "2020-12-25", "2021-01-05"),
        ]);

        AssertPair(result, 1, 2, (10, 12));
    }

    [Fact]
    public void FindLongestPairs_EmployeeWithBackToBackAssignmentsOnSameProject_CountsBothPeriods()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(1, 10, "2020-01-11", "2020-01-20"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-20"),
        ]);

        AssertPair(result, 1, 2, (10, 20));
    }

    [Fact]
    public void FindLongestPairs_EmployeeOnExtraProjectWithoutPartner_DoesNotCountExtraProjectDays()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(1, 20, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
        ]);

        AssertPair(result, 1, 2, (10, 10));
    }

    [Fact]
    public void FindLongestPairs_ThreeEmployeesOnOneProject_ReturnsPairWithLongestOverlap()
    {
        // 1&2 share Jan 1-10 (10), 1&3 share Jan 5-31 (27), 2&3 share Jan 5-10 (6).
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-31"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(3, 10, "2020-01-05", "2020-01-31"),
        ]);

        AssertPair(result, 1, 3, (10, 27));
    }

    [Fact]
    public void FindLongestPairs_SeveralAssignmentsEndingOnSameDay_ReturnsPairWithLongestOverlap()
    {
        // 1&2 share 3 days, 1&3 share 3 days, 2&3 share 6 days (Jan 5-10).
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-08", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-05", "2020-01-10"),
            CreateAssignment(3, 10, "2020-01-01", "2020-01-10"),
        ]);

        AssertPair(result, 2, 3, (10, 6));
    }

    [Fact]
    public void FindLongestPairs_MultiplePairsOnDifferentProjects_ReturnsPairWithLongestOverlap()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-05"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-05"),
            CreateAssignment(3, 20, "2020-02-01", "2020-02-28"),
            CreateAssignment(4, 20, "2020-02-01", "2020-02-28"),
        ]);

        AssertPair(result, 3, 4, (20, 28));
    }

    [Fact]
    public void FindLongestPairs_PairsWithEqualDays_ReturnsAllOfThem()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(7, 20, "2020-01-01", "2020-01-10"),
            CreateAssignment(8, 20, "2020-01-01", "2020-01-10"),
            CreateAssignment(3, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(4, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(5, 30, "2020-01-01", "2020-01-10"),
            CreateAssignment(6, 30, "2020-01-01", "2020-01-10"),
        ]);

        AssertPairs(
            result,
            Pair(3, 4, (10, 10)),
            Pair(5, 6, (30, 10)),
            Pair(7, 8, (20, 10)));
    }

    [Fact]
    public void FindLongestPairs_ThreeEmployeesWithIdenticalPeriods_ReturnsEveryCombination()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(3, 10, "2020-01-01", "2020-01-10"),
        ]);

        AssertPairs(
            result,
            Pair(1, 2, (10, 10)),
            Pair(1, 3, (10, 10)),
            Pair(2, 3, (10, 10)));
    }

    [Fact]
    public void FindLongestPairs_LongestPairFoundBeforeShorterOnes_StaysTheOnlyResult()
    {
        // Pairs are finalized in order of end date: 1&2 (10 days), then 3&4 (5 days), then 5&6 (7 days).
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(3, 20, "2020-01-16", "2020-01-20"),
            CreateAssignment(4, 20, "2020-01-16", "2020-01-20"),
            CreateAssignment(5, 30, "2020-01-24", "2020-01-30"),
            CreateAssignment(6, 30, "2020-01-24", "2020-01-30"),
        ]);

        AssertPair(result, 1, 2, (10, 10));
    }

    [Fact]
    public void FindLongestPairs_ShorterPairsAreNotReturnedAlongsideTies()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(3, 20, "2020-01-01", "2020-01-10"),
            CreateAssignment(4, 20, "2020-01-01", "2020-01-10"),
            CreateAssignment(5, 30, "2020-01-01", "2020-01-05"),
            CreateAssignment(6, 30, "2020-01-01", "2020-01-05"),
        ]);

        AssertPairs(result, Pair(1, 2, (10, 10)), Pair(3, 4, (20, 10)));
    }

    [Fact]
    public void FindLongestPairs_SamePairWithTwoSeparatePeriodsOnOneProject_SumsDaysInOneProjectEntry()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(1, 10, "2020-06-01", "2020-06-05"),
            CreateAssignment(2, 10, "2020-06-01", "2020-06-05"),
        ]);

        AssertPair(result, 1, 2, (10, 15));
    }

    [Fact]
    public void FindLongestPairs_SamePairOnSeveralProjects_SumsDaysAcrossProjects()
    {
        // 1&2 work together 10 days on project 10 and 10 days on project 20 (20 total),
        // which beats 1&3 who share 15 days on project 30.
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(1, 20, "2020-02-01", "2020-02-10"),
            CreateAssignment(2, 20, "2020-02-01", "2020-02-10"),
            CreateAssignment(1, 30, "2020-03-01", "2020-03-15"),
            CreateAssignment(3, 30, "2020-03-01", "2020-03-15"),
        ]);

        AssertPair(result, 1, 2, (10, 10), (20, 10));
    }

    [Fact]
    public void FindLongestPairs_SamePairOnSeveralProjectsAtTheSameTime_SumsDaysPerProject()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(2, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(1, 20, "2020-01-06", "2020-01-15"),
            CreateAssignment(2, 20, "2020-01-06", "2020-01-15"),
        ]);

        AssertPair(result, 1, 2, (10, 10), (20, 10));
    }

    [Fact]
    public void FindLongestPairs_ProjectsAndPairs_AreOrderedById()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(8, 30, "2020-01-01", "2020-01-10"),
            CreateAssignment(7, 30, "2020-01-01", "2020-01-10"),
            CreateAssignment(8, 20, "2020-02-01", "2020-02-10"),
            CreateAssignment(7, 20, "2020-02-01", "2020-02-10"),
            CreateAssignment(2, 10, "2020-03-01", "2020-03-20"),
            CreateAssignment(1, 10, "2020-03-01", "2020-03-20"),
        ]);

        AssertPairs(result, Pair(1, 2, (10, 20)), Pair(7, 8, (20, 10), (30, 10)));
    }

    [Fact]
    public void FindLongestPairs_InputInReverseOrder_ReturnsSameResult()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(2, 10, "2020-01-05", "2020-01-20"),
            CreateAssignment(1, 10, "2020-01-01", "2020-01-10"),
        ]);

        AssertPair(result, 1, 2, (10, 6));
    }

    [Fact]
    public void FindLongestPairs_EmployeeIdsInDescendingOrder_ReportsSmallerIdFirst()
    {
        var result = Finder.FindLongestPairs(
        [
            CreateAssignment(9, 10, "2020-01-01", "2020-01-10"),
            CreateAssignment(3, 10, "2020-01-01", "2020-01-10"),
        ]);

        AssertPair(result, 3, 9, (10, 10));
    }

    private static Assignment CreateAssignment(int employeeId, int projectId, string from, string to) =>
        new(employeeId, projectId, DateOnly.Parse(from), DateOnly.Parse(to));

    private static void AssertPair(IList<EmployeePair> actual, int employee1, int employee2, params (int ProjectId, int Days)[] projects) =>
        AssertPairs(actual, Pair(employee1, employee2, projects));

    // Pairs are expected ordered by employee IDs and projects by project ID.
    private static void AssertPairs(IList<EmployeePair> actual, params string[] expected) =>
        Assert.Equal(
            expected,
            actual.Select(p => Describe(p.Employee1, p.Employee2, p.Days, p.Projects)));

    // The expected total is the sum of the expected project days.
    private static string Pair(int employee1, int employee2, params (int ProjectId, int Days)[] projects) =>
        Describe(
            employee1,
            employee2,
            projects.Sum(p => p.Days),
            projects.Select(p => new ProjectOverlap(p.ProjectId, p.Days)));

    private static string Describe(int employee1, int employee2, int days, IEnumerable<ProjectOverlap> projects) =>
        $"{employee1}&{employee2}: {days} days [{string.Join(", ", projects.Select(p => $"project {p.ProjectId}: {p.Days}"))}]";
}
