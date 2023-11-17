using Microsoft.EntityFrameworkCore;
using Weasel.Audit.Extensions;
using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Services;

public interface IAuditStateManager
{
    IPostponedAuditManager PostponedAuditManager { get; }
    IAuditSchemeManager SchemeManager { get; }
    IAuditStateStorage GlobalStateStorage { get; }
    void PushStates();
    void CommitState<TAudit>(string entityId, TAudit data)
        where TAudit : class, IIntKeyedEntity;
    Task<TAudit> GetLastAction<T, TAudit>(DbContext context, T model)
        where T : class, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity;
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

    public async Task<TAudit> GetLastAction<T, TAudit>(DbContext context, T model)
        where T : class, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }
        TAudit? state = null;
        string entityId = context.GetAuditEntityId(model);
        if (PlannedStateStorage.TryGetValue<TAudit>(entityId, out state))
        {
            return state;
        }
        if (GlobalStateStorage.TryGetValue<TAudit>(entityId, out state))
        {
            return state;
        }
        state = await SearchInDataBase<TAudit>(context, entityId);
        if (state != null)
        {
            GlobalStateStorage.PushState(entityId, state);
            return state;
        }
        state = await PostponedAuditManager.PlanAknowledgeAsync<T, TAudit>(context, model);
        CommitState(entityId, state);
        return state;
    }
    public void CommitState<TAudit>(string entityId, TAudit data) where TAudit : class, IIntKeyedEntity
        => PlannedStateStorage.PushState(new AuditStateCacheKey(typeof(TAudit), entityId), data);
    public void PushStates()
        => GlobalStateStorage.JoinStorage(PlannedStateStorage);
    private async Task<TAudit?> SearchInDataBase<TAudit>(DbContext context, string id) where TAudit : class, IIntKeyedEntity
    {
        List<Enum> types = SchemeManager.GetTypeActions<TAudit>();
        if (types.Count == 0)
        {
            throw new Exception($"Can't find action type enum values for type {typeof(TAudit)}!");
        }
        IAuditAction? dataAction = await context.Set<IAuditAction>().AsNoTracking().OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => types.Contains(x.Type) && x.EntityId == id);
        if (dataAction == null)
        {
            return null;
        }
        if (dataAction.NewDataId != null)
        {
            return await context.Set<TAudit>().FirstOrDefaultAsync(x => x.Id == dataAction.NewDataId.Value);
        }
        else if (dataAction.OldDataId != null)
        {
            return await context.Set<TAudit>().FirstOrDefaultAsync(x => x.Id == dataAction.OldDataId.Value);
        }
        return null;
    }
}
