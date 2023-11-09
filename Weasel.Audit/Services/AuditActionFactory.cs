using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Services;

public interface IAuditActionFactory
{
    public IAuditAction CreateIntAuditAction(Enum type, int entityId, int? userId, int? newDataId, int? oldDataId, string? overrideLogin, Enum? overrideColor, DateTime dateTime);
}