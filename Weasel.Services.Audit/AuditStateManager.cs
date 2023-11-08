using ECRF.Tools.Actions.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Weasel.Audit.Interfaces;

namespace Weasel.Services.Audit;

public interface IAuditStateManager
{
    IPostponedAuditManager PostponedAuditManager { get; }
    IAuditSchemeManager SchemeManager { get; }
    IAuditStateStorage GlobalStateStorage { get; }
    void PushStates();
    void CommitState<TAction>(int entityId, TAction data)
        where TAction : class, IIntKeyedEntity;
    Task<TAction> GetLastAction<T, TAction>(DbContext context, T model)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity;
}
public sealed class AuditStateManager : IAuditStateManager
{
    public IAuditStateStorage PlannedStateStorage { get; private set; }
    public IPostponedAuditManager PostponedAuditManager { get; private set; }
    public IAuditSchemeManager SchemeManager => PostponedAuditManager.SchemeManager;
    public IAuditStateStorage GlobalStateStorage => PostponedAuditManager.GlobalStateStorage;
    public AuditStateManager(IPostponedAuditManager posponedAuditManager)
    {
        PlannedStateStorage = new AuditStateStorage();
        PostponedAuditManager = posponedAuditManager;
    }

    public async Task<TAction> GetLastAction<T, TAction>(DbContext context, T model)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity
    {
        TAction? state = null;
        if (PlannedStateStorage.TryGetValue<TAction>(model.Id, out state))
        {
            return state;
        }
        if (GlobalStateStorage.TryGetValue<TAction>(model.Id, out state))
        {
            return state;
        }
        state = await SearchInDataBase<TAction>(context, model.Id);
        if (state != null)
        {
            GlobalStateStorage.PushState(model.Id, state);
            return state;
        }
        state = await PostponedAuditManager.PlanAknowledgeAsync<T, TAction>(context, model);
        CommitState(model.Id, state);
        return state;
    }
    public void CommitState<TAction>(int entityId, TAction data)
        where TAction : class, IIntKeyedEntity
        => PlannedStateStorage.PushState(new AuditStateCacheKey(typeof(TAction), entityId), data);
    public void PushStates()
        => GlobalStateStorage.JoinStorage(PlannedStateStorage);
    private async Task<TAction?> SearchInDataBase<TAction>(DbContext context, int id)
        where TAction : class, IIntKeyedEntity
    {
        List<Enum> types = SchemeManager.GetTypeActions<TAction>();
        if (types.Count == 0)
        {
            throw new Exception($"Can't find action type enum values for type {typeof(TAction)}!");
        }
        IAuditAction? dataAction = await context.Set<IAuditAction>().AsNoTracking().OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => types.Contains(x.Type) && x.EntityId == id);
        if (dataAction == null)
        {
            return null;
        }
        if (dataAction.NewDataId != null)
        {
            return await context.Set<TAction>().FirstOrDefaultAsync(x => x.Id == dataAction.NewDataId.Value);
        }
        else if (dataAction.OldDataId != null)
        {
            return await context.Set<TAction>().FirstOrDefaultAsync(x => x.Id == dataAction.OldDataId.Value);
        }
        return null;
    }
}
