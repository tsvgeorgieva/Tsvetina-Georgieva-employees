# Employees

Finds the pair of employees who have worked together on common projects for the longest total time. Input is a CSV file of project assignments; there is a web UI and a console app.

## Run

Requires the .NET 10 SDK.

```
dotnet run --project src/Employees.Web                      # web UI: choose a CSV file, see the result in a table
dotnet run --project src/Employees.Console -- src/Employees.Console/sample.csv    # console, with the sample file
dotnet test src/Employees.slnx                              # unit tests
```

The web page and the console show one row per pair and common project: `Employee ID #1`, `Employee ID #2`, `Project ID` and `Days worked together`. Invalid rows are reported with their line numbers instead.

With the sample file the result is two tied pairs, the first one with three projects:

```
Employee ID #1, Employee ID #2, Project ID, Days worked together
143, 218, 12, 36
143, 218, 14, 10
143, 218, 15, 10
301, 302, 20, 30
301, 302, 21, 26
```

## Input

CSV with no header, columns `EmpID, ProjectID, DateFrom, DateTo`:

```
143,12,2013-11-01,2014-01-05
218,10,2012-05-16,NULL
```

- `DateTo` empty or `NULL` means the assignment is still ongoing and ends today.
- Several date formats are accepted (`2020-03-05`, `20200305`, `5.3.2020`, `5 Mar 2020`, ...). Dates with `/`, `-` or `.` and the year last are read **day-first**, since month-first would be ambiguous.
- The whole file is validated first. If any row is invalid, nothing is calculated and all errors are returned with line numbers.

## Rules

- Two employees overlap when they were on the **same project** at the same time. Only the overlapping days count.
- Dates are inclusive calendar days: Jan 1 to Jan 10 is 10 days, and touching on a single day is 1 shared day.
- A pair's days are summed over all their common projects. The pair with the highest total wins, and the result lists its days per project.
- Ties: **all** tied pairs are returned. No overlap at all gives an empty result.
- Output order is deterministic: pairs are ordered by `Employee ID #1`, then `Employee ID #2`, and each pair's projects by `Project ID`.

## Algorithm

A sweep line, per project (`PairFinder`):

1. Group assignments by project, since only those can overlap.
2. Turn each assignment into a start event and an end event, sorted by date. On equal dates starts come first, which gives the inclusive "touching" behaviour.
3. Walk the events keeping a set of active assignments. When an assignment ends, it overlaps with everything still active; add the shared days (`end - later start + 1`) to that employee pair's total and to the pair's days for the current project.
4. One pass over the totals keeps the maximum and all pairs equal to it, together with their per-project days. Only these few result pairs are sorted (by employee IDs, projects by project ID), so ordering adds no real cost.

Each overlap is counted exactly once: whichever assignment ends first finds the other still active. Cost is O(n log n) for sorting plus the number of overlapping pairs, which any solution must at least output. This avoids comparing every assignment with every other one.

## Design

```
Employees.Core          parsing and pair finding, no UI dependency
Employees.Console       command line front end
Employees.Web           ASP.NET Core MVC front end (one controller, one view)
Employees.Core.Tests    xUnit tests for the core
```

- The core is built from small interfaces with constructor injection: `IAssignmentParser`, `IPairFinder` and `IPairReportService`. The web app registers them in `Program.cs`; the console app wires them by hand.
- Today's date (for open-ended assignments) comes from an injected `TimeProvider`, so tests use a fixed date instead of depending on the clock.
- An `EmployeePair` carries the total `Days` (used to pick the longest pair) and a `Projects` list of `ProjectOverlap(ProjectId, Days)`. Per-project days are tracked in a private `PairOverlap` inside `PairFinder`, with one dictionary lookup per overlap, so the cost stays the same as before.
- The parser takes a `TextReader`, not a file path, so it does not care where the data comes from (file, upload, string in a test). It is synchronous on purpose: it is CPU-bound work, and the upload is already handled by the framework.

## Known limitations and open questions

- **Same day on several projects.** Days are summed per project, so a day the pair shares on two projects counts twice. If each calendar day should count once per pair, the date ranges per pair would need to be merged first.
- **Overlapping assignments of the same employee on one project.** They are not treated as a special case; an employee is never paired with themselves, but shared days are counted once per overlapping assignment. This should either be rejected as invalid data or confirmed as legitimate.
