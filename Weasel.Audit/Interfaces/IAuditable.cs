using Microsoft.EntityFrameworkCore;

namespace Weasel.Audit.Interfaces;

public interface IAuditable<TAuditResult, TAuditAction, TEnum>
    where TAuditResult : class, IAuditResult<TAuditAction, TEnum>
    where TAuditAction : class, IAuditAction<TEnum>
	where TEnum : struct, Enum
{
    Task<TAuditResult> AuditAsync(DbContext context);
}