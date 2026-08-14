namespace Convergex.Application.Helpers;

public static class TimeZoneHelper
{
    public static TimeZoneInfo GetTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    public static DateTime ToLocal(
        DateTime utcDateTime,
        string? timeZoneId)
    {
        var timeZone = GetTimeZone(timeZoneId);

        var utc = DateTime.SpecifyKind(
            utcDateTime,
            DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(
            utc,
            timeZone);
    }

    public static DateTime LocalToUtc(
        DateTime localDateTime,
        string? timeZoneId)
    {
        var timeZone = GetTimeZone(timeZoneId);

        var local = DateTime.SpecifyKind(
            localDateTime,
            DateTimeKind.Unspecified);

        return TimeZoneInfo.ConvertTimeToUtc(
            local,
            timeZone);
    }
}