namespace Employees.Core;

public class PairFinder : IPairFinder
{
    public List<EmployeePair> FindLongestPairs(IEnumerable<Assignment> assignments)
    {
        var longestPairs = new List<EmployeePair>();
        long maxDays = 0;

        foreach (var ((employee1, employee2), days) in FindPairOverlaps(assignments))
        {
            if (days > maxDays)
            {
                longestPairs = [new EmployeePair(employee1, employee2, days)];
                maxDays = days;
            }
            else if (days == maxDays)
            {
                longestPairs.Add(new EmployeePair(employee1, employee2, days));
            }
        }

        return longestPairs;
    }

    private static Dictionary<(int Employee1, int Employee2), long> FindPairOverlaps(IEnumerable<Assignment> assignments)
    {
        var pairOverlaps = new Dictionary<(int Employee1, int Employee2), long>();

        foreach (var projectAssignments in assignments.GroupBy(a => a.ProjectId))
        {
            var activeAssignments = new HashSet<Assignment>();

            foreach (var point in ToSortedPoints(projectAssignments))
            {
                if (point.Type == AssignmentPointType.Start)
                {
                    activeAssignments.Add(point.Assignment);
                    continue;
                }

                activeAssignments.Remove(point.Assignment);
                AccumulateOverlaps(pairOverlaps, point.Assignment, activeAssignments);
            }
        }

        return pairOverlaps;
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
            if (activeAssignment.EmployeeId == ending.EmployeeId)
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
