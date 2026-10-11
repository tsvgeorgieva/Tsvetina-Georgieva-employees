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

## Assumptions

- The data in the input file is correct and is not validated. In particular, every assignment is assumed to start on or before its end date; results for assignments that start after they end are undefined.

## Open requirements questions

- **Should a calendar day be counted once per pair, even if the pair works together on several projects at the same time?** Currently the overlap days of each project are simply summed, so a day on which a pair shares two projects counts twice. If each calendar day must count only once per pair, the overlapping date ranges per pair need to be merged before counting.
- **Can one employee have overlapping assignments on the same project?** The code does not treat this as a special case: an employee is never paired with themselves, but if such assignments overlap with a colleague's, the shared days are counted once per overlapping assignment. If this situation is invalid in the source data (for example, an employee cannot be assigned to the same project twice at the same time), it should be rejected with a validation error, or confirmed as legitimate. This needs to be clarified with the requirement owner.
