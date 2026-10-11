namespace Employees.Core;

public class PairReportService(IAssignmentParser parser, IPairFinder pairFinder, TimeProvider timeProvider) : IPairReportService
{
    public PairReport Run(TextReader reader)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        var parsed = parser.Parse(reader, today);

        return parsed.IsValid
            ? new PairReport(pairFinder.FindLongestPairs(parsed.Assignments), [])
            : new PairReport([], parsed.Errors);
    }
}
