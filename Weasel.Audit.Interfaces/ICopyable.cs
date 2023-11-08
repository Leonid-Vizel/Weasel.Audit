using Microsoft.EntityFrameworkCore;

namespace ECRF.Tools.Actions.Interfaces;

public interface IAuditable<TAudit>
{
    Task<TAudit> AuditAsync(DbContext context);
}