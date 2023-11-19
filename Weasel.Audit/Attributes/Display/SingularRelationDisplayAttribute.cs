using System.Reflection;
using Weasel.Audit.Enums;

namespace Weasel.Audit.Attributes.Display;

[AttributeUsage(AttributeTargets.Property)]
public class SingularRelationDisplayAttribute : AuditDisplayStrategyAttribute
{
    public Type RelatingType { get; private set; }
    public SingularRelationDisplayAttribute(Type relatingType)
    {
        RelatingType = relatingType;
    }

    public override object? FormatValue(PropertyInfo info, object? declare, object? value)
        => value;
    public override AuditPropertyDisplayMode GetDisplayMode(PropertyInfo info, object? declare, object? value)
        => AuditPropertyDisplayMode.SingularRelation;
    public override Type? GetCollectionType(PropertyInfo info, object? declare, object? value)
        => null;
    public override string GetRowName(int index, PropertyInfo info, object? declare, object? value)
        => "";
}
