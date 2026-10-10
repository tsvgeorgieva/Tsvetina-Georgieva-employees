namespace Employees.Core;

public record EmployeePair(int Employee1, int Employee2, long Days)
{
    public long Days { get; set; } = Days;

    public static EmployeePair Between(int employeeA, int employeeB, long days) =>
        new(Math.Min(employeeA, employeeB), Math.Max(employeeA, employeeB), days);

    public virtual bool Equals(EmployeePair? other) =>
        other is not null && Employee1 == other.Employee1 && Employee2 == other.Employee2;

    public override int GetHashCode() => HashCode.Combine(Employee1, Employee2);
}
