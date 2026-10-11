namespace Employees.Core;

public record PairReport(IReadOnlyList<EmployeePair> Pairs, IReadOnlyList<CsvParseError> Errors)
{
    public bool IsValid => Errors.Count == 0;
}
