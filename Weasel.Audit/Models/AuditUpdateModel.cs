using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Models;

public sealed class AuditUpdateModel<TAction, TRow, TEnum> : AuditIndexModel<TAction, TRow, TEnum>
    where TAction : class, IAuditAction<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
	where TEnum : struct, Enum
{
    public List<AuditPropertyDisplayModel> Old { get; set; } = null!;
    public List<AuditPropertyDisplayModel> Update { get; set; } = null!;
}
