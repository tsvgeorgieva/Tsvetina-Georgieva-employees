namespace Employees.Core;

public interface IPairReportService
{
    PairReport Run(TextReader reader);
}
