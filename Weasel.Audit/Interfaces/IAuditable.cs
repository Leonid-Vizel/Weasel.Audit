using Microsoft.EntityFrameworkCore;

namespace Weasel.Audit.Interfaces;

public interface IAuditable<TResult, TAction, TRow, TEnum>
    where TResult : class, IAuditResult<TAction, TRow, TEnum>
    where TAction : class, IAuditAction<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
    where TEnum : struct, Enum
{
    Task<TResult> AuditAsync(DbContext context);
}