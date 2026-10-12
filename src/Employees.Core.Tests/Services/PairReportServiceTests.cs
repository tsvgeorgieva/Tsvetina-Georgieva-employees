namespace Employees.Core.Tests.Services;

public class PairReportServiceTests
{
    private static readonly PairReportService Service = new(
        new AssignmentCsvParser(), new PairFinder(), new FixedTimeProvider(new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc)));

    [Fact]
    public void Run_ValidCsv_ReturnsLongestPair()
    {
        var report = Run(
            "1,10,2020-01-01,2020-01-10\n" +
            "2,10,2020-01-06,2020-01-20\n" +
            "3,10,2020-01-10,2020-01-12");

        Assert.True(report.IsValid);
        var pair = Assert.Single(report.Pairs);
        Assert.Equal((1, 2, 5), (pair.Employee1, pair.Employee2, pair.Days));
        Assert.Equal([new ProjectOverlap(10, 5)], pair.Projects);
    }

    [Fact]
    public void Run_OngoingAssignment_UsesTodayAsEndDate()
    {
        var report = Run(
            "1,10,2024-06-01,NULL\n" +
            "2,10,2024-06-10,2024-12-31");

        var pair = Assert.Single(report.Pairs);
        Assert.Equal((1, 2, 6), (pair.Employee1, pair.Employee2, pair.Days));
    }

    [Fact]
    public void Run_InvalidCsv_ReturnsErrorsAndNoPairs()
    {
        var report = Run(
            "1,10,2020-01-01,2020-01-10\n" +
            "2,10,bad,2020-01-20");

        Assert.False(report.IsValid);
        Assert.Empty(report.Pairs);
        Assert.Equal(2, Assert.Single(report.Errors).Line);
    }

    [Fact]
    public void Run_NoOverlap_ReturnsEmptyPairsWithoutErrors()
    {
        var report = Run(
            "1,10,2020-01-01,2020-01-10\n" +
            "2,10,2020-02-01,2020-02-10");

        Assert.True(report.IsValid);
        Assert.Empty(report.Pairs);
    }

    private static PairReport Run(string csv) =>
        Service.Run(new StringReader(csv));
}
