using System.Collections;
using System.Reflection;
using Weasel.Audit.Enums;
using Weasel.Tools.Extensions.Common;

namespace Weasel.Audit.Attributes.Display;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class StandartAuditDisplayAttribute : AuditDisplayStrategyAttribute
{
    private static readonly Type[] _fieldTypes =
    [
        typeof(int),
        typeof(int?),
        typeof(long),
        typeof(long?),
        typeof(uint),
        typeof(uint?),
        typeof(ulong),
        typeof(ulong?),
        typeof(byte),
        typeof(byte?),
        typeof(sbyte),
        typeof(sbyte?),
        typeof(short),
        typeof(short?),
        typeof(bool),
        typeof(bool?),
        typeof(float),
        typeof(float?),
        typeof(double),
        typeof(double?),
        typeof(decimal),
        typeof(decimal?),
        typeof(string),
        typeof(char),
        typeof(char?),
        typeof(DateTime),
        typeof(DateTime?),
        typeof(DateOnly),
        typeof(DateOnly?),
        typeof(TimeOnly),
        typeof(TimeOnly?),
    ];
    public string? NullValue { get; private set; }
    public string? TrueValue { get; private set; }
    public string? FalseValue { get; private set; }
    public string? DateTimeFormat { get; private set; }
    public string? DateOnlyFormat { get; private set; }
    public string? TimeOnlyFormat { get; private set; }
    public Type? EnumRowType { get; private set; }
    public string? RowName { get; set; }
    public string? RowSeparator { get; set; }
    public int RowIndexOffset { get; set; }
    public StandartAuditDisplayAttribute(string? nullValue = "Не указано", string? trueValue = "Да", string? falseValue = "Нет",
        string? dateTimeFormat = "dd.MM.yyyy HH:mm:ss", string? dateOnlyFormat = "dd.MM.yyyy", string? timeOnlyFormat = "HH:mm:ss",
        Type? enumRowType = null, string rowName = "Строка", string rowSeparator = " #", int rowIndexOffset = 1)
    {
        NullValue = nullValue;
        TrueValue = trueValue;
        FalseValue = falseValue;
        DateTimeFormat = dateTimeFormat;
        DateOnlyFormat = dateOnlyFormat;
        TimeOnlyFormat = timeOnlyFormat;
        EnumRowType = enumRowType;
        RowName = rowName;
        RowSeparator = rowSeparator;
        RowIndexOffset = rowIndexOffset;
    }

    public override object? FormatValue(PropertyInfo info, object? declare, object? value)
    {
        if (value == null)
        {
            return NullValue;
        }
        if (value is bool boolValue)
        {
            return boolValue ? TrueValue : FalseValue;
        }
        if (value is DateTime dateTimeValue)
        {
            return dateTimeValue.ToString(DateTimeFormat);
        }
        if (value is DateOnly dateOnlyValue)
        {
            return dateOnlyValue.ToString(DateOnlyFormat);
        }
        if (value is TimeOnly timeOnlyValue)
        {
            return timeOnlyValue.ToString(TimeOnlyFormat);
        }
        if (value is Enum enumValue)
        {
            return enumValue.GetDisplayName() ?? enumValue.ToString();
        }
        return value;
    }
    public override AuditPropertyDisplayMode GetDisplayMode(PropertyInfo info, object? declare, object? value)
    {
        Type type = value?.GetType() ?? info.PropertyType;
        if (_fieldTypes.Contains(type) || type.IsEnum)
        {
            return AuditPropertyDisplayMode.Field;
        }
        if (type.IsAssignableTo(typeof(ICollection)))
        {
            return AuditPropertyDisplayMode.Collection;
        }
        return AuditPropertyDisplayMode.Object;
    }
    public override string GetRowName(int index, PropertyInfo info, object? declare, object? value)
    {
        index += RowIndexOffset;
        if (EnumRowType != null)
        {
            var obj = (Enum)Enum.ToObject(EnumRowType, index);
            return obj.GetDisplayName() ?? obj.ToString();
        }
        return $"{RowName}{RowSeparator}{index}";
    }
    public override Type? GetCollectionType(PropertyInfo info, object? declare, object? value)
    {
        if (GetDisplayMode(info, declare, value) == AuditPropertyDisplayMode.Collection)
        {
            return info.PropertyType.GenericTypeArguments[0];
        }
        return null;
    }
}
