using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Models;

public abstract class AuditIndexModel<TAction, TRow, TEnum>
    where TAction : class, IAuditAction<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
	where TEnum : struct, Enum
{
    public TAction Action { get; set; } = null!;
}
