namespace Employees.Core;

public record CsvParseResult(IReadOnlyList<Assignment> Assignments, IReadOnlyList<CsvParseError> Errors)
{
    public bool IsValid => Errors.Count == 0;
}
