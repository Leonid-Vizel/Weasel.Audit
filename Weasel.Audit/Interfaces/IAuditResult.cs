using Weasel.Audit.Attributes.Display;

namespace Weasel.Audit.Interfaces;

public interface IAuditResult<TAuditAction> : IIntKeyedEntity where TAuditAction : class, IAuditAction
{
    [IgnoreAuditDisplay]
    int ActionId { get; set; }
    [IgnoreAuditDisplay]
    TAuditAction Action { get; set; }
}
