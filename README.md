# Tsvetina-Georgieva-employees
Application that identifies the pair of employees who have worked together on common projects for the longest period of time

## How the longest pair is determined

- Two employees are a pair when they worked on the same project during overlapping dates. Only the overlapping days count.
- Dates are inclusive and counted as **calendar days**: an overlap from Jan 1 to Jan 10 is 10 days, and assignments that touch on a single day share 1 day.
- Days a pair spends together are added up across all common projects.
- The pair with the most calendar days in total, across all common projects, is returned.
- **If several pairs are tied for the longest total, all of them are returned**, not just one. The result is a list, and the order of pairs within it is not guaranteed.
- Each pair is reported with the lower employee ID first.
- If no two employees ever overlapped, an empty list is returned.

## Algorithm

Comparing every assignment with every other one would take quadratic time and would mostly compare assignments that can never overlap. Instead, the code uses a **sweep line** over the assignment dates, done separately for each project (see `PairFinder`).

1. **Group by project.** Only assignments on the same project can overlap, so each project is processed on its own. Pairs are never compared across projects, but their days are added into one shared total per pair.
2. **Turn assignments into events.** Every assignment produces a *start* event on its first day and an *end* event on its last day. The events of the project are sorted by date. On the same date, starts come before ends, which is what makes assignments that touch on a single day count as 1 shared day (dates are inclusive).
3. **Sweep through the events,** keeping the set of assignments that are currently active:
   - On a *start* event, the assignment is added to the active set.
   - On an *end* event, the assignment is removed from the set, and then compared with every assignment that is still active (skipping the same employee). Each of those overlaps with it, and the shared days are `end of the ending assignment - later of the two start dates + 1`. The days are added to the total of that employee pair (stored with the lower ID first).
4. **Pick the winners.** One pass over the pair totals keeps the maximum, and all pairs equal to it.

Why each overlap is counted exactly once: when two assignments overlap, whichever one ends first finds the other still active and records the overlap. When the other one ends later, the first is already gone from the active set, so nothing is counted twice.

Complexity: sorting the events is O(n log n) for n assignments. The comparison work depends on how many assignments on the same project are active at the same time, since every one of them forms a pair with the ending assignment. For realistic data this is small, so the running time is close to O(n log n). In the worst case, when many employees are on one project at once, the output itself contains a pair for each combination of them, so the work cannot be lower than that.

## Assumptions

- The data in the input file is correct and is not validated. In particular, every assignment is assumed to start on or before its end date; results for assignments that start after they end are undefined.

## Open requirements questions

- **Should a calendar day be counted once per pair, even if the pair works together on several projects at the same time?** Currently the overlap days of each project are simply summed, so a day on which a pair shares two projects counts twice. If each calendar day must count only once per pair, the overlapping date ranges per pair need to be merged before counting.
- **Can one employee have overlapping assignments on the same project?** The code does not treat this as a special case: an employee is never paired with themselves, but if such assignments overlap with a colleague's, the shared days are counted once per overlapping assignment. If this situation is invalid in the source data (for example, an employee cannot be assigned to the same project twice at the same time), it should be rejected with a validation error, or confirmed as legitimate. This needs to be clarified with the requirement owner.
