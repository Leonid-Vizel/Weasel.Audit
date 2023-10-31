using System.Text;

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
    public static int Age(this DateTime birthDate)
        => Age(birthDate, DateTime.Today);
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
    public static int Age(this DateOnly birthDate)
        => Age(birthDate, DateOnly.FromDateTime(DateTime.Today));
    public static string ToRussian(this TimeSpan span, bool backThen = true)
    {
        int days = span.Days;
        int hours = span.Hours;
        int minutes = span.Minutes;
        int seconds = span.Seconds;
        StringBuilder builder = new StringBuilder();
        if (days > 0)
        {
            builder.Append($"{days} {days.Inflect("дней", "день", "дня")} ");
        }
        if (hours > 0)
        {
            builder.Append($"{hours} {hours.Inflect("часов", "час", "часа")} ");
        }
        if (minutes > 0)
        {
            builder.Append($"{minutes} {minutes.Inflect("минут", "минуту", "минуты")} ");
        }
        builder.Append($"{seconds} {seconds.Inflect("секунд", "секунду", "секунды")}");
        return backThen ? builder.Append(" назад").ToString() : builder.ToString();
    }
    public static DateTime AddBusinessDays(this DateTime date, int days)
    {
        if (days < 0)
        {
            throw new ArgumentException("days cannot be negative", "days");
        }
        if (days == 0) return date;
        if (date.DayOfWeek == DayOfWeek.Saturday)
        {
            date = date.AddDays(2);
            days -= 1;
        }
        else if (date.DayOfWeek == DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
            days -= 1;
        }
        date = date.AddDays(days / 5 * 7);
        int extraDays = days % 5;
        if ((int)date.DayOfWeek + extraDays > 5)
        {
            extraDays += 2;
        }
        return date.AddDays(extraDays);
    }
    public static DateTime GetQuarterStart(this int quarter, int year)
        => new DateTime(year, quarter * 3 - 2, 1);
    public static DateOnly GetDateOnlyQuarterStart(this int quarter, int year)
        => new DateOnly(year, quarter * 3 - 2, 1);

    public static DateTime GetQuarterEnd(this int quarter, int year)
    {
        int endMonth = quarter * 3;
        int endDay = DateTime.DaysInMonth(year, endMonth);
        return new DateTime(year, endMonth, endDay);
    }
    public static DateOnly GetDateOnlyQuarterEnd(this int quarter, int year)
    {
        int endMonth = quarter * 3;
        int endDay = DateTime.DaysInMonth(year, endMonth);
        return new DateOnly(year, endMonth, endDay);
    }

    public static DateTime GetYearEnd(this int year)
        => new DateTime(year, 12, DateTime.DaysInMonth(year, 12));
    public static DateOnly GetDateOnlyYearEnd(this int year)
        => new DateOnly(year, 12, DateTime.DaysInMonth(year, 12));
    public static int GetCurrentQuarter(this DateTime date)
        => (date.Month / 4) + 1;
    public static int GetCurrentQuarter(this DateOnly date)
        => (date.Month / 4) + 1;
}