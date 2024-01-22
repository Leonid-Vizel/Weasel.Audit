using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Models;

public sealed class AuditHistoryStateModel<TAction, TRow, TEnum> : AuditInfoModel<TAction, TRow, TEnum>
    where TAction : class, IAuditAction<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
	where TEnum : struct, Enum
{
    public bool Changed { get; set; }
}
