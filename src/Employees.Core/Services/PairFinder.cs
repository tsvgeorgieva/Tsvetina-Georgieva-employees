namespace Employees.Core;

public static class PairFinder
{
    public static EmployeePair? FindLongestPair(IEnumerable<Assignment> assignments)
    {
        var assignmentList = assignments.ToList();
        ValidateDateRanges(assignmentList);

        return FindPairOverlaps(assignmentList)
            .OrderByDescending(pair => pair.Days)
            .ThenBy(pair => pair.Employee1)
            .ThenBy(pair => pair.Employee2)
            .FirstOrDefault();
    }

    private static void ValidateDateRanges(IEnumerable<Assignment> assignments)
    {
        var invalid = assignments.FirstOrDefault(a => a.From > a.To);
        if (invalid is not null)
        {
            throw new ArgumentException(
                $"Assignment of employee {invalid.EmployeeId} on project {invalid.ProjectId} starts ({invalid.From}) after it ends ({invalid.To}).",
                nameof(assignments));
        }
    }

    private static IEnumerable<EmployeePair> FindPairOverlaps(IEnumerable<Assignment> assignments)
    {
        var pairOverlaps = new Dictionary<(int Employee1, int Employee2), List<DateRange>>();
        var activeAssignments = new HashSet<Assignment>();

        foreach (var point in ToSortedPoints(assignments))
        {
            if (point.Type == AssignmentPointType.Start)
            {
                activeAssignments.Add(point.Assignment);
                continue;
            }

            activeAssignments.Remove(point.Assignment);
            AccumulateOverlaps(pairOverlaps, point, activeAssignments);
        }

        return pairOverlaps.Select(entry => new EmployeePair(
            entry.Key.Employee1,
            entry.Key.Employee2,
            CountCalendarDays(entry.Value)));
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
        Dictionary<(int Employee1, int Employee2), List<DateRange>> pairOverlaps,
        AssignmentPoint currentPoint,
        IEnumerable<Assignment> activeAssignments)
    {
        var ending = currentPoint.Assignment;

        foreach (var activeAssignment in activeAssignments)
        {
            if (activeAssignment.ProjectId != ending.ProjectId || activeAssignment.EmployeeId == ending.EmployeeId)
            {
                continue;
            }

            var key = (Math.Min(ending.EmployeeId, activeAssignment.EmployeeId), Math.Max(ending.EmployeeId, activeAssignment.EmployeeId));
            var overlap = new DateRange(
                ending.From > activeAssignment.From ? ending.From : activeAssignment.From,
                currentPoint.Date);

            if (!pairOverlaps.TryGetValue(key, out var ranges))
            {
                pairOverlaps[key] = ranges = [];
            }

            ranges.Add(overlap);
        }
    }

    // A pair can work together on several projects during the same dates, so the ranges are
    // merged before counting to make sure each calendar day is counted only once.
    private static long CountCalendarDays(List<DateRange> ranges)
    {
        long totalDays = 0;
        DateRange? current = null;

        foreach (var range in ranges.OrderBy(r => r.From))
        {
            if (current is null)
            {
                current = range;
            }
            else if (range.From > current.Value.To)
            {
                totalDays += current.Value.Days;
                current = range;
            }
            else if (range.To > current.Value.To)
            {
                current = current.Value with { To = range.To };
            }
        }

        return totalDays + (current?.Days ?? 0);
    }

    private readonly record struct DateRange(DateOnly From, DateOnly To)
    {
        public long Days => To.DayNumber - From.DayNumber + 1;
    }
}
