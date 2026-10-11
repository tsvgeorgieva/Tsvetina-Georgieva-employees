namespace Employees.Core.Tests.Services;

public class AssignmentCsvParserTests
{
    private static readonly DateOnly Today = new(2024, 6, 15);

    [Fact]
    public void Parse_ValidRows_ParsesAllRows()
    {
        var result = Parse("1,10,2020-01-01,2020-01-31\n2,10,2020-01-15,2020-02-15");

        Assert.True(result.IsValid);
        Assert.Equal(2, result.Assignments.Count);
    }

    [Theory]
    [InlineData("2020-03-05")]
    [InlineData("2020/03/05")]
    [InlineData("05/03/2020")]
    [InlineData("5-3-2020")]
    [InlineData("05.03.2020")]
    [InlineData("5 Mar 2020")]
    [InlineData("20200305")]
    public void Parse_SupportedDateFormat_ParsesDate(string date)
    {
        var result = Parse($"1,10,{date},2021-01-01");

        Assert.True(result.IsValid);
        Assert.Equal(new DateOnly(2020, 3, 5), result.Assignments[0].From);
    }

    [Theory]
    [InlineData("NULL")]
    [InlineData("null")]
    [InlineData("")]
    public void Parse_NullOrEmptyDateTo_UsesToday(string dateTo)
    {
        var result = Parse($"1,10,2020-01-01,{dateTo}");

        Assert.True(result.IsValid);
        Assert.Equal(Today, result.Assignments[0].To);
    }

    [Fact]
    public void Parse_BlankLines_AreIgnored()
    {
        var result = Parse("1,10,2020-01-01,2020-01-31\n\n   \n2,10,2020-01-01,2020-01-31\n");

        Assert.True(result.IsValid);
        Assert.Equal(2, result.Assignments.Count);
    }

    [Fact]
    public void Parse_FieldsWithSpaces_AreTrimmed()
    {
        var result = Parse("1, 10 , 2020-01-01, 2020-01-31 ");

        Assert.True(result.IsValid);
        Assert.Equal(new Assignment(1, 10, new DateOnly(2020, 1, 1), new DateOnly(2020, 1, 31)), result.Assignments[0]);
    }

    [Fact]
    public void Parse_EmptyInput_ReturnsNoAssignmentsAndNoErrors()
    {
        var result = Parse("");

        Assert.True(result.IsValid);
        Assert.Empty(result.Assignments);
    }

    [Theory]
    [InlineData("abc,10,2020-01-01,2020-01-31", "EmpID")]
    [InlineData("0,10,2020-01-01,2020-01-31", "EmpID")]
    [InlineData("-5,10,2020-01-01,2020-01-31", "EmpID")]
    [InlineData("1,x,2020-01-01,2020-01-31", "ProjectID")]
    [InlineData("1,10,not-a-date,2020-01-31", "DateFrom")]
    [InlineData("1,10,2020-01-01,31/31/2020", "DateTo")]
    [InlineData("1,10,2020-02-30,2020-03-31", "DateFrom")]
    public void Parse_InvalidField_ReportsErrorNamingTheField(string row, string field)
    {
        var result = Parse($"2,10,2020-01-01,2020-01-31\n{row}");

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(2, error.Line);
        Assert.Contains(field, error.Message);
    }

    [Fact]
    public void Parse_DateFromAfterDateTo_ReportsError()
    {
        var result = Parse("1,10,2020-02-01,2020-01-01");

        var error = Assert.Single(result.Errors);
        Assert.Equal(1, error.Line);
        Assert.Contains("after", error.Message);
    }

    [Fact]
    public void Parse_OngoingAssignmentStartingAfterToday_ReportsError()
    {
        var result = Parse("1,10,2025-01-01,NULL");

        Assert.Single(result.Errors);
    }

    [Theory]
    [InlineData("1,10,2020-01-01")]
    [InlineData("1,10,2020-01-01,2020-01-31,extra")]
    public void Parse_WrongColumnCount_ReportsError(string row)
    {
        var result = Parse(row);

        var error = Assert.Single(result.Errors);
        Assert.Contains("columns", error.Message);
    }

    [Fact]
    public void Parse_SeveralInvalidRows_ReportsAllWithLineNumbers()
    {
        var result = Parse(
            "1,10,2020-01-01,2020-01-31\n" +
            "x,10,2020-01-01,2020-01-31\n" +
            "\n" +
            "2,10,2020-03-01,2020-01-31\n" +
            "3,10,2020-01-01");

        Assert.Equal([2, 4, 5], result.Errors.Select(e => e.Line));
        Assert.Single(result.Assignments);
    }

    private static CsvParseResult Parse(string csv) =>
        AssignmentCsvParser.Parse(new StringReader(csv), Today);
}
