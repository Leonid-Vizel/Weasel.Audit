using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Models;

public abstract class AuditIndexModel<TAuditAction>
    where TAuditAction : class, IAuditAction
{
    public TAuditAction Action { get; set; } = null!;
}
