namespace Employees.Core;

public record CsvParseResult(IList<Assignment> Assignments, IList<CsvParseError> Errors)
{
    public bool IsValid => Errors.Count == 0;
}
