using Weasel.Audit.Attributes.Display;

namespace Weasel.Audit.Interfaces;

public interface IAuditResult<TAction, TRow, TEnum> : IIntKeyedEntity
    where TAction : class, IAuditAction<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
	where TEnum : struct, Enum
{
    [IgnoreAuditDisplay]
    int ActionId { get; set; }
    [IgnoreAuditDisplay]
    TAction Action { get; set; }
}
