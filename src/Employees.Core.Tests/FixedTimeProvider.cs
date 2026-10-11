namespace Employees.Core.Tests;

public class FixedTimeProvider(DateTime utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(utcNow);

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
