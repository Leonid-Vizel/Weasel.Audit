using Microsoft.EntityFrameworkCore;

namespace Weasel.Audit.Interfaces;

public interface IAuditable<TAuditResult, TAuditAction>
    where TAuditResult : class, IAuditResult<TAuditAction>
    where TAuditAction : class, IAuditAction
{
    Task<TAuditResult> AuditAsync(DbContext context);
}