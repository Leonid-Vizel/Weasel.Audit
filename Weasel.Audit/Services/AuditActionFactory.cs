using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Services;

public interface IAuditActionFactory
{
    public IAuditAction CreateAuditAction(Enum type, string entityId, object? additional = null, int? newDataId = null, int? oldDataId = null, string? overrideLogin = null, Enum? overrideColor = null);
}