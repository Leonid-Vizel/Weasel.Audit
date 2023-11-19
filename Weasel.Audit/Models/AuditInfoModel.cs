using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Models;

public class AuditInfoModel<TAuditAction, TEnum> : AuditIndexModel<TAuditAction, TEnum>
    where TAuditAction : class, IAuditAction<TEnum>
	where TEnum : struct, Enum
{
    public List<AuditPropertyDisplayModel> Items { get; set; } = null!;
}
