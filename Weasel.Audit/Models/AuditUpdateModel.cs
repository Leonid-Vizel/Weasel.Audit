using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Models;

public sealed class AuditUpdateModel<TAuditAction> : AuditIndexModel<TAuditAction>
    where TAuditAction : class, IAuditAction
{
    public List<AuditPropertyDisplayModel> Old { get; set; } = null!;
    public List<AuditPropertyDisplayModel> Update { get; set; } = null!;
}
