using System.Reflection;
using Weasel.Audit.Enums;
using Weasel.Audit.Services;

namespace Weasel.Audit.Attributes.Display;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public sealed class IgnoreAuditDisplayAttribute : AuditDisplayStrategyAttribute
{
    public override object? FormatValue(PropertyInfo info, object? declare, object? value)
        => null;
    public override Type? GetCollectionType(PropertyInfo info, object? declare, object? value)
        => null;
    public override AuditPropertyDisplayMode GetDisplayMode(PropertyInfo info, object? declare, object? value)
        => AuditPropertyDisplayMode.None;
    public override string GetRowName(int index, PropertyInfo info, object? declare, object? value)
        => string.Empty;
}
