using System.Reflection;
using Weasel.Audit.Enums;

namespace Weasel.Audit.Attributes.Display;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public abstract class AuditDisplayStrategyAttribute : Attribute
{
    public abstract AuditPropertyDisplayMode GetDisplayMode(PropertyInfo info, object? declare, object? value);
    public abstract string GetRowName(int index, PropertyInfo info, object? declare, object? value);
    public abstract object? FormatValue(PropertyInfo info, object? declare, object? value);
    public abstract Type? GetCollectionType(PropertyInfo info, object? declare, object? value);
}
