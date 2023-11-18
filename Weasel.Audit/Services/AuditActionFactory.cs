using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Services;

public interface IAuditActionFactory<TAuditAction> where TAuditAction : class, IAuditAction
{
    public TAuditAction CreateAuditAction(Enum type, string entityId, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null);
}