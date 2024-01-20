using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Services;

public interface IAuditActionFactory<TAction, TRow, TEnum>
    where TAction : class, IAuditAction<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
    where TEnum : struct, Enum
{
    public TAction CreateAuditAction(TRow row, string entityId, object? additional = null);
}