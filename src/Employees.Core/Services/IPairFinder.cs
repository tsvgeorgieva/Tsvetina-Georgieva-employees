namespace Employees.Core;

public interface IPairFinder
{
    List<EmployeePair> FindLongestPairs(IEnumerable<Assignment> assignments);
}
