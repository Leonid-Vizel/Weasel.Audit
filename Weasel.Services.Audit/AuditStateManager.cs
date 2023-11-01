using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Weasel.Services.Audit;

public sealed class AuditStateManager
{
    private readonly ConcurrentDictionary<Type, object> _cachedStates;
    public AuditStateManager()
    {
        _cachedStates = new ConcurrentDictionary<Type, object>();
    }

    public static async Task<TAction?> GetLastAction<TAction>(DbContext database, int id) where TAction : class, IIndexable
    {
        List<DataActionType> types = GetTypeActions<TAction>();
        if (types.Count == 0 || types[0] == DataActionType.Unknown)
        {
            return null;
        }
        DataAction? dataAction = await database.Set<DataAction>().AsNoTracking().OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => types.Contains(x.Type) && x.EntityId == id);
        if (dataAction == null)
        {
            return null;
        }
        if (dataAction.NewDataId != null)
        {
            return await database.Set<TAction>().FirstOrDefaultAsync(x => x.Id == dataAction.NewDataId.Value);
        }
        else if (dataAction.OldDataId != null)
        {
            return await database.Set<TAction>().FirstOrDefaultAsync(x => x.Id == dataAction.OldDataId.Value);
        }
        return null;
    }
}
