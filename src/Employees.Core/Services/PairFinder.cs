namespace Employees.Core;

public class PairFinder : IPairFinder
{
    public IList<EmployeePair> FindLongestPairs(IEnumerable<Assignment> assignments)
    {
        var longestPairs = new List<EmployeePair>();
        int maxDays = 0;

        foreach (var ((employee1, employee2), overlap) in FindPairOverlaps(assignments))
        {
            if (overlap.Days > maxDays)
            {
                longestPairs = [new EmployeePair(employee1, employee2, overlap.Days, overlap.Projects)];
                maxDays = overlap.Days;
            }
            else if (overlap.Days == maxDays)
            {
                longestPairs.Add(new EmployeePair(employee1, employee2, overlap.Days, overlap.Projects));
            }
        }

        longestPairs.Sort((a, b) => (a.Employee1, a.Employee2).CompareTo((b.Employee1, b.Employee2)));

        return longestPairs;
    }

    private static Dictionary<(int Employee1, int Employee2), PairOverlap> FindPairOverlaps(IEnumerable<Assignment> assignments)
    {
        var pairOverlaps = new Dictionary<(int Employee1, int Employee2), PairOverlap>();

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
        Dictionary<(int Employee1, int Employee2), PairOverlap> pairOverlaps,
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

            if (!pairOverlaps.TryGetValue(key, out var pairOverlap))
            {
                pairOverlaps[key] = pairOverlap = new PairOverlap();
            }

            pairOverlap.Add(ending.ProjectId, overlapDays);
        }
    }

    private record AssignmentPoint(Assignment Assignment, DateOnly Date, AssignmentPointType Type);

    private enum AssignmentPointType { Start, End }

    private class PairOverlap
    {
        private readonly Dictionary<int, int> _daysByProject = [];

        public int Days { get; private set; }

        public IEnumerable<ProjectOverlap> Projects =>
            _daysByProject.OrderBy(p => p.Key).Select(p => new ProjectOverlap(p.Key, p.Value)).ToList();

        public void Add(int projectId, int days)
        {
            Days += days;
            _daysByProject[projectId] = _daysByProject.GetValueOrDefault(projectId) + days;
        }
    }
}
