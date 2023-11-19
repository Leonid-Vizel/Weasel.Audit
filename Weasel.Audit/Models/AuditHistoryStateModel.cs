using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Models;

public sealed class AuditHistoryStateModel<TAuditAction, TEnum> : AuditInfoModel<TAuditAction, TEnum>
    where TAuditAction : class, IAuditAction<TEnum>
	where TEnum : struct, Enum
{
    public bool Changed { get; set; }
}
