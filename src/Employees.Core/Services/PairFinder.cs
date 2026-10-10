namespace Employees.Core;

public static class PairFinder
{
    public static EmployeePair? FindLongestPair(IEnumerable<Assignment> assignments)
        => FindPairOverlaps(assignments).MaxBy(pair => pair.Days);

    private static HashSet<EmployeePair> FindPairOverlaps(IEnumerable<Assignment> assignments)
    {
        var pairs = new HashSet<EmployeePair>();
        var activeAssignments = new HashSet<Assignment>();

        foreach (var point in ToSortedPoints(MergeOverlappingPerEmployeeProject(assignments)))
        {
            if (point.Type == AssignmentPointType.Start)
            {
                activeAssignments.Add(point.Assignment);
                continue;
            }

            activeAssignments.Remove(point.Assignment);
            AccumulateOverlaps(pairs, point, activeAssignments);
        }

        return pairs;
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

    private static IEnumerable<Assignment> MergeOverlappingPerEmployeeProject(IEnumerable<Assignment> assignments)
    {
        foreach (var group in assignments.GroupBy(a => (a.EmployeeId, a.ProjectId)))
        {
            Assignment? previousAssignment = null;
            foreach (var currentAssignment in group.OrderBy(a => a.From))
            {
                if (previousAssignment is null)
                {
                    previousAssignment = currentAssignment;
                    continue;
                }

                if (currentAssignment.From <= previousAssignment.To)
                {
                    previousAssignment = previousAssignment with { To = currentAssignment.To > previousAssignment.To ? currentAssignment.To : previousAssignment.To };
                }
                else
                {
                    yield return previousAssignment;
                    previousAssignment = currentAssignment;
                }
            }

            yield return previousAssignment!;
        }
    }

    private static void AccumulateOverlaps(
        HashSet<EmployeePair> pairs,
        AssignmentPoint currentPoint,
        IEnumerable<Assignment> activeAssignments)
    {
        foreach (var activeAssignment in activeAssignments)
        {
            if (activeAssignment.ProjectId != currentPoint.Assignment.ProjectId)
            {
                continue;
            }

            EmployeePair pair = CalculateOverlap(currentPoint, activeAssignment);

            if (pairs.TryGetValue(pair, out var existing))
            {
                existing.Days += pair.Days;
            }
            else
            {
                pairs.Add(pair);
            }
        }
    }

    private static EmployeePair CalculateOverlap(AssignmentPoint currentPoint, Assignment activeAssignment)
    {
        var fromDayNumber = Math.Max(currentPoint.Assignment.From.DayNumber, activeAssignment.From.DayNumber);
        var totalDays = currentPoint.Date.DayNumber - fromDayNumber + 1;
        return EmployeePair.Between(currentPoint.Assignment.EmployeeId, activeAssignment.EmployeeId, totalDays);
    }
}
