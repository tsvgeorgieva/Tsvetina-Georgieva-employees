namespace Employees.Core;

public record EmployeePair(int Employee1, int Employee2, int Days, IEnumerable<ProjectOverlap> Projects);
