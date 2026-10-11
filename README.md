# Employees

Finds the pair of employees who have worked together on common projects for the longest total time. Input is a CSV file of project assignments; there is a web UI and a console app.

## Run

Requires the .NET 10 SDK.

```
dotnet run --project src/Employees.Web                      # web UI: choose a CSV file, see the result in a table
dotnet run --project src/Employees.Console -- src/Employees.Console/sample.csv    # console, with the sample file
dotnet test src/Employees.slnx                              # unit tests
```

The web page shows `Employee ID #1`, `Employee ID #2` and `Days worked together`. Invalid rows are reported with their line numbers instead.

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
- A pair's days are summed over all their common projects. The pair with the highest total wins.
- Ties: **all** tied pairs are returned. No overlap at all gives an empty result.

## Algorithm

A sweep line, per project (`PairFinder`):

1. Group assignments by project, since only those can overlap.
2. Turn each assignment into a start event and an end event, sorted by date. On equal dates starts come first, which gives the inclusive "touching" behaviour.
3. Walk the events keeping a set of active assignments. When an assignment ends, it overlaps with everything still active; add the shared days (`end - later start + 1`) to that employee pair's total.
4. One pass over the totals keeps the maximum and all pairs equal to it.

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
- The parser takes a `TextReader`, not a file path, so it does not care where the data comes from (file, upload, string in a test). It is synchronous on purpose: it is CPU-bound work, and the upload is already handled by the framework.

## Known limitations and open questions

- **No Project ID column in the UI.** The task asks for a grid of common projects (Employee ID #1, Employee ID #2, Project ID, Days worked). The core currently returns only the total days per pair, so the UI shows that. Showing days per project needs a small change in `PairFinder`, which already tracks overlaps per project internally.
- **Same day on several projects.** Days are summed per project, so a day the pair shares on two projects counts twice. If each calendar day should count once per pair, the date ranges per pair would need to be merged first.
- **Overlapping assignments of the same employee on one project.** They are not treated as a special case; an employee is never paired with themselves, but shared days are counted once per overlapping assignment. This should either be rejected as invalid data or confirmed as legitimate.
