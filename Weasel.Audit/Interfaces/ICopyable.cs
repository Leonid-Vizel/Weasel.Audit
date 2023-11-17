using Microsoft.EntityFrameworkCore;
using Weasel.Audit.Services;

namespace Weasel.Audit.Interfaces;

public interface IAuditable<TAudit> where TAudit: IIntKeyedEntity
{
    Task<TAudit> AuditAsync(DbContext context, IPostponedAuditManager manager);
}