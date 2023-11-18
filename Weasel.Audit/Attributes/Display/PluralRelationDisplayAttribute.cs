using System.Reflection;
using Weasel.Audit.Enums;

namespace Weasel.Audit.Attributes.Display;

[AttributeUsage(AttributeTargets.Property)]
public class PluralRelationDisplayAttribute : AuditDisplayStrategyAttribute
{
    public Type RelatingType { get; private set; }
    public string EntityIdPropertyName { get; private set; }

    public PluralRelationDisplayAttribute(Type relatingType, string entityIdPropertyName)
    {
        RelatingType = relatingType;
        EntityIdPropertyName = entityIdPropertyName;
    }

    public override object? FormatValue(PropertyInfo info, object? declare, object? value)
        => value;
    public override AuditPropertyDisplayMode GetDisplayMode(PropertyInfo info, object? declare, object? value)
        => AuditPropertyDisplayMode.PluralRelation;
    public override Type? GetCollectionType(PropertyInfo info, object? declare, object? value)
        => null;
    public override string GetRowName(int index, PropertyInfo info, object? declare, object? value)
        => "";
}
