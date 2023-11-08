using Weasel.Audit.Interfaces;

namespace Weasel.Services.Audit;

public interface IAuditActionFactory
{
    public IAuditAction CreateAuditAction(Enum type, int entityId, int? userId, int? newDataId, int? oldDataId, string? overrideLogin, Enum? overrideColor, DateTime dateTime);
}