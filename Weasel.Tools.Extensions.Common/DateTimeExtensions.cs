namespace Weasel.Tools.Extensions.Common;

public static class DateTimeExtensions
{
    private static readonly TimeOnly _emptyTime = new TimeOnly(0, 0, 0);

    public static DateOnly? ToDateOnly(this DateTime? dateTime)
        => dateTime == null ? null : dateTime.Value.ToDateOnly();
    public static DateOnly ToDateOnly(this DateTime dateTime)
        => DateOnly.FromDateTime(dateTime);
    public static TimeOnly? ToTimeOnly(this DateTime? dateTime)
        => dateTime == null ? null : dateTime.Value.ToTimeOnly();
    public static TimeOnly ToTimeOnly(this DateTime dateTime)
        => TimeOnly.FromDateTime(dateTime);
    public static DateTime? ToDateTime(this DateOnly? date)
        => date == null ? null : date.Value.ToDateTime(_emptyTime);
    public static DateTime ToDateTime(this DateOnly date)
        => date.ToDateTime(_emptyTime);
    public static DateTime? ToDateTime(this TimeOnly? time)
        => time == null ? null : time.Value.ToDateTime();
    public static DateTime ToDateTime(this TimeOnly time)
        => new DateTime(1, 1, 1, time.Hour, time.Minute, time.Second);
    public static int Age(this DateTime birthDate)
    {
        return Age(birthDate, DateTime.Today);
    }
    public static int Age(this DateTime birthDate, DateTime laterDate)
    {
        int age;
        age = laterDate.Year - birthDate.Year;

        if (age > 0)
        {
            age -= Convert.ToInt32(laterDate.Date < birthDate.Date.AddYears(age));
        }
        else
        {
            age = 0;
        }

        return age;
    }
    public static int Age(this DateOnly birthDate)
    {
        return Age(birthDate, DateOnly.FromDateTime(DateTime.Today));
    }
    public static int Age(this DateOnly birthDate, DateOnly laterDate)
    {
        int age;
        age = laterDate.Year - birthDate.Year;

        if (age > 0)
        {
            age -= Convert.ToInt32(laterDate < birthDate.AddYears(age));
        }
        else
        {
            age = 0;
        }

        return age;
    }
}
