using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Services;

public interface IAuditActionFactory<TAuditAction, TEnum>
    where TAuditAction : class, IAuditAction<TEnum>
	where TEnum : struct, Enum
{
    public TAuditAction CreateAuditAction(TEnum type, string entityId, object? additional = null);
}