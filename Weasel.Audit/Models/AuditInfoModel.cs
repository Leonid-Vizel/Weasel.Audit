using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Models;

public class AuditInfoModel<TAuditAction> : AuditIndexModel<TAuditAction>
    where TAuditAction : class, IAuditAction
{
    public List<AuditPropertyDisplayModel> Items { get; set; } = null!;
}
