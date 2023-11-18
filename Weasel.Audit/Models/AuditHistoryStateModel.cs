using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Models;

public sealed class AuditHistoryStateModel<TAuditAction> : AuditInfoModel<TAuditAction>
    where TAuditAction : class, IAuditAction
{
    public bool Changed { get; set; }
}
