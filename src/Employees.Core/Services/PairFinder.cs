namespace Employees.Core;

public static class PairFinder
{
    public static EmployeePair? FindLongestPair(IEnumerable<Assignment> assignments)
        => FindPairOverlaps(assignments)
            .OrderByDescending(pair => pair.Days)
            .ThenBy(pair => pair.Employee1)
            .ThenBy(pair => pair.Employee2)
            .FirstOrDefault();

    private static IEnumerable<EmployeePair> FindPairOverlaps(IEnumerable<Assignment> assignments)
    {
        var pairOverlaps = new Dictionary<(int Employee1, int Employee2), long>();
        var activeAssignments = new HashSet<Assignment>();

        foreach (var point in ToSortedPoints(assignments))
        {
            if (point.Type == AssignmentPointType.Start)
            {
                activeAssignments.Add(point.Assignment);
                continue;
            }

            activeAssignments.Remove(point.Assignment);
            AccumulateOverlaps(pairOverlaps, point.Assignment, activeAssignments);
        }

        return pairOverlaps.Select(entry => new EmployeePair(
            entry.Key.Employee1,
            entry.Key.Employee2,
            entry.Value));
    }

    private static IEnumerable<AssignmentPoint> ToSortedPoints(IEnumerable<Assignment> assignments)
    {
        var points = new List<AssignmentPoint>();
        foreach (var a in assignments)
        {
            points.Add(new AssignmentPoint(a, a.From, AssignmentPointType.Start));
            points.Add(new AssignmentPoint(a, a.To, AssignmentPointType.End));
        }

        return points.OrderBy(point => point.Date).ThenBy(point => point.Type);
    }

    private static void AccumulateOverlaps(
        Dictionary<(int Employee1, int Employee2), long> pairOverlaps,
        Assignment ending,
        IEnumerable<Assignment> activeAssignments)
    {
        foreach (var activeAssignment in activeAssignments)
        {
            if (activeAssignment.ProjectId != ending.ProjectId || activeAssignment.EmployeeId == ending.EmployeeId)
            {
                continue;
            }

            var key = (Math.Min(ending.EmployeeId, activeAssignment.EmployeeId), Math.Max(ending.EmployeeId, activeAssignment.EmployeeId));
            var overlapStart = ending.From > activeAssignment.From ? ending.From : activeAssignment.From;
            var overlapDays = ending.To.DayNumber - overlapStart.DayNumber + 1;

            pairOverlaps[key] = pairOverlaps.GetValueOrDefault(key) + overlapDays;
        }
    }
}
