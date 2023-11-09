namespace Weasel.Audit.Attributes.Formatters;

[AttributeUsage(AttributeTargets.Property)]
public abstract class AuditValueFormatterAttribute : Attribute
{
    public abstract object? FormatValue(object? value);
}
