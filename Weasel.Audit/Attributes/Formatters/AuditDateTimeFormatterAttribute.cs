namespace Weasel.Audit.Attributes.Formatters;

[AttributeUsage(AttributeTargets.Property)]
public sealed class AuditDateTimeFormatterAttribute : AuditValueFormatterAttribute
{
    public string Format { get; private set; }
    public AuditDateTimeFormatterAttribute(string format)
    {
        Format = format;
    }
    public override object? FormatValue(object? value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString(Format);
        }
        if (value is DateOnly dateOnly)
        {
            return dateOnly.ToString(Format);
        }
        return value;
    }
}
