using Weasel.Audit.Attributes.Display;

namespace Weasel.Audit.Interfaces;

public interface IAuditResult<TAuditAction, TEnum> : IIntKeyedEntity
    where TAuditAction : class, IAuditAction<TEnum>
	where TEnum : struct, Enum
{
    [IgnoreAuditDisplay]
    int ActionId { get; set; }
    [IgnoreAuditDisplay]
    TAuditAction Action { get; set; }
}
