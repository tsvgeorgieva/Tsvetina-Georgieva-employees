namespace Employees.Core;

public interface IAssignmentParser
{
    CsvParseResult Parse(TextReader reader, DateOnly today);
}
