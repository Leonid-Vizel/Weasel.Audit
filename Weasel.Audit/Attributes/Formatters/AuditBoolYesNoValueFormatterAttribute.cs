using Weasel.Tools.Extensions.Common;

namespace Weasel.Audit.Attributes.Formatters;

[AttributeUsage(AttributeTargets.Property)]
public sealed class AuditBoolYesNoValueFormatterAttribute : AuditValueFormatterAttribute
{
    public string? NullValue { get; private set; }
    public string YesValue { get; private set; }
    public string NoValue { get; private set; }
    public AuditBoolYesNoValueFormatterAttribute(string? nullValue = "Не указано", string yesValue = "Да", string noValue = "Нет")
    {
        NullValue = nullValue;
        YesValue = yesValue;
        NoValue = noValue;
    }
    public override object? FormatValue(object? value)
    {
        if (value is bool)
        {
            bool? boolValue = (bool)value;
            return boolValue.ToYesNoString(NullValue, YesValue, NoValue);
        }
        if (value is bool?)
        {
            bool? boolValue = (bool?)value;
            return boolValue.ToYesNoString(NullValue, YesValue, NoValue);
        }
        return value;
    }
}
