namespace Employees.Core;

public record PairReport(IList<EmployeePair> Pairs, IList<CsvParseError> Errors)
{
    public bool IsValid => Errors.Count == 0;
}
