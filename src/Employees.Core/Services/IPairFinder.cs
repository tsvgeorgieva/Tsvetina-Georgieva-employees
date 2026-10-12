namespace Employees.Core;

public interface IPairFinder
{
    IList<EmployeePair> FindLongestPairs(IEnumerable<Assignment> assignments);
}
