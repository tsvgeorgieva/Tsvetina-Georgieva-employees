namespace Employees.Core;

public static class PairReportService
{
    public static PairReport Run(TextReader reader, DateOnly today)
    {
        var parsed = AssignmentCsvParser.Parse(reader, today);

        return parsed.IsValid
            ? new PairReport(PairFinder.FindLongestPairs(parsed.Assignments), [])
            : new PairReport([], parsed.Errors);
    }
}
