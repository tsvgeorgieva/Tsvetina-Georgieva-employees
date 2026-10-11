using System.Globalization;

namespace Employees.Core;

public static class AssignmentCsvParser
{
    private const int ColumnCount = 4;

    // Slash dates are day-first: month-first would be ambiguous with them.
    private static readonly string[] DateFormats =
    [
        "yyyy-MM-dd",
        "yyyy/MM/dd",
        "yyyy.MM.dd",
        "yyyyMMdd",
        "d/M/yyyy",
        "d-M-yyyy",
        "d.M.yyyy",
        "d MMM yyyy",
        "d MMMM yyyy",
    ];

    public static CsvParseResult Parse(TextReader reader, DateOnly today)
    {
        var assignments = new List<Assignment>();
        var errors = new List<CsvParseError>();
        var lineNumber = 0;

        while (reader.ReadLine() is { } line)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var fields = SplitLine(line);
            if (fields.Count != ColumnCount)
            {
                errors.Add(new CsvParseError(lineNumber, $"Expected {ColumnCount} columns but found {fields.Count}."));
                continue;
            }

            if (TryParseRow(fields, today, out var assignment, out var error))
            {
                assignments.Add(assignment);
            }
            else
            {
                errors.Add(new CsvParseError(lineNumber, error));
            }
        }

        return new CsvParseResult(assignments, errors);
    }

    private static bool TryParseRow(List<string> fields, DateOnly today, out Assignment assignment, out string error)
    {
        assignment = null!;
        var problems = new List<string>();

        var employeeIdValid = TryParseId(fields[0], out var employeeId);
        if (!employeeIdValid)
        {
            problems.Add($"Invalid EmpID '{fields[0]}': expected a positive integer.");
        }

        var projectIdValid = TryParseId(fields[1], out var projectId);
        if (!projectIdValid)
        {
            problems.Add($"Invalid ProjectID '{fields[1]}': expected a positive integer.");
        }

        var fromValid = TryParseDate(fields[2], out var from);
        if (!fromValid)
        {
            problems.Add($"Invalid DateFrom '{fields[2]}': unrecognized date.");
        }

        var toValid = true;
        DateOnly to = today;
        var toText = fields[3];
        if (toText.Length > 0 && !toText.Equals("NULL", StringComparison.OrdinalIgnoreCase))
        {
            toValid = TryParseDate(toText, out to);
            if (!toValid)
            {
                problems.Add($"Invalid DateTo '{toText}': unrecognized date.");
            }
        }

        if (fromValid && toValid && from > to)
        {
            problems.Add($"DateFrom {from:yyyy-MM-dd} is after DateTo {to:yyyy-MM-dd}.");
        }

        if (problems.Count > 0)
        {
            error = string.Join(" ", problems);
            return false;
        }

        assignment = new Assignment(employeeId, projectId, from, to);
        error = string.Empty;
        return true;
    }

    private static bool TryParseId(string text, out int id) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out id) && id > 0;

    private static bool TryParseDate(string text, out DateOnly date) =>
        DateOnly.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    private static List<string> SplitLine(string line)
        => [.. line.Split(',', StringSplitOptions.TrimEntries)];
}
