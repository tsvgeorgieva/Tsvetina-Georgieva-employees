namespace Employees.Core;

public record Assignment(int EmployeeId, int ProjectId, DateOnly From, DateOnly To);
