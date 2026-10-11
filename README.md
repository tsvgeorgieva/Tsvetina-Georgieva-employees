# Tsvetina-Georgieva-employees
Application that identifies the pair of employees who have worked together on common projects for the longest period of time


## How the longest pair is determined

- Two employees are a pair when they worked on the same project during overlapping dates. Only the overlapping days count.
- Dates are inclusive and counted as **calendar days**: an overlap from Jan 1 to Jan 10 is 10 days, and assignments that touch on a single day share 1 day.
- A day is counted once per pair, no matter how many projects cover it. Days a pair spends together on different projects are added up only when they fall on different calendar days.
- The pair with the most calendar days in total, across all common projects, is returned. If several pairs have the same total, the one with the lowest employee IDs wins (by the lower ID first, then the higher one).
- If no two employees ever overlapped, no pair is returned.

## Assumptions

- The data in the input file is correct and is not validated. In particular, every assignment is assumed to start on or before its end date; results for assignments that start after they end are undefined.

## Open requirements questions

- **Can one employee have overlapping assignments on the same project?** The code does not treat this as a special case: such assignments are handled like any others, an employee is never paired with themselves, and a day shared with a colleague is counted once per pair. If this situation is actually invalid in the source data (for example, an employee cannot be assigned to the same project twice at the same time), it should be rejected with a validation error, or confirmed as legitimate. This needs to be clarified with the requirement owner.
