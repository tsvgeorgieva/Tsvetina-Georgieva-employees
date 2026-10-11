using Employees.Core;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: Employees.Console <path-to-assignments.csv>");
    return 2;
}

if (!File.Exists(args[0]))
{
    Console.Error.WriteLine($"File not found: {args[0]}");
    return 2;
}

using var reader = new StreamReader(args[0]);
var report = PairReportService.Run(reader, DateOnly.FromDateTime(DateTime.Today));

if (!report.IsValid)
{
    Console.Error.WriteLine($"The file contains {report.Errors.Count} invalid row(s):");
    foreach (var error in report.Errors)
    {
        Console.Error.WriteLine($"Line {error.Line}: {error.Message}");
    }

    return 1;
}

if (report.Pairs.Count == 0)
{
    Console.WriteLine("No employees worked together on a common project.");
    return 0;
}

Console.WriteLine("Employee ID #1, Employee ID #2, Days worked together");
foreach (var pair in report.Pairs)
{
    Console.WriteLine($"{pair.Employee1}, {pair.Employee2}, {pair.Days}");
}

return 0;
