# Tsvetina-Georgieva-employees
Application that identifies the pair of employees who have worked together on common projects for the longest period of time


## How the longest pair is determined

- Two employees are a pair when they worked on the same project during overlapping dates. Only the overlapping days count.
- Dates are inclusive and counted as **calendar days**: an overlap from Jan 1 to Jan 10 is 10 days, and assignments that touch on a single day share 1 day.
- A day is counted once per pair, no matter how many projects or overlapping assignments cover it. Days a pair spends together on different projects are added up only when they fall on different calendar days.
- The pair with the most calendar days in total, across all common projects, is returned. If several pairs have the same total, the one with the lowest employee IDs wins (by the lower ID first, then the higher one).
- If no two employees ever overlapped, no pair is returned.
- An assignment whose start date is after its end date is invalid and causes an `ArgumentException`.
