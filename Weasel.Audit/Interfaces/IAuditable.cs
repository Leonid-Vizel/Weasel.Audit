using Microsoft.EntityFrameworkCore;

namespace Weasel.Audit.Interfaces;

public interface IAuditable<TAudit> where TAudit: IIntKeyedEntity
{
    Task<TAudit> AuditAsync(DbContext context);
}