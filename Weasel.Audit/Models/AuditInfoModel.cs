using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Models;

public class AuditInfoModel<TAction, TRow, TEnum> : AuditIndexModel<TAction, TRow, TEnum>
    where TAction : class, IAuditAction<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
	where TEnum : struct, Enum
{
    public List<AuditPropertyDisplayModel> Items { get; set; } = null!;
}
