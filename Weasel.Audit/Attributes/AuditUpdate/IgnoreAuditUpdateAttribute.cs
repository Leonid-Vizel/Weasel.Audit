using Microsoft.EntityFrameworkCore;

namespace Weasel.Audit.Attributes.AuditUpdate;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public sealed class IgnoreAuditUpdateAttribute : AuditUpdateStrategyAttribute
{
    public override bool Compare(DbContext context, object? old, object? update, object? oldValue, object? updateValue)
        => true;
    public override object? SetValue(DbContext context, object? old, object? update, object? oldValue, object? updateValue)
        => oldValue;
}
