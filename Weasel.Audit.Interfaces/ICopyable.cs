using Microsoft.EntityFrameworkCore;

namespace ECRF.Tools.Actions.Interfaces;

public interface IAuditable<TAction>
{
    Task<TAction> AuditAsync(DbContext context);
}