using Microsoft.EntityFrameworkCore;
using Weasel.Audit.Extensions;
using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Services;

public struct PostponedModelData<T> where T : class
{
    public T Model { get; private set; }
    public Enum ActionType { get; private set; }
    public object? Additional { get; private set; }
    public string? OverrideLogin { get; private set; }
    public Enum? OverrideColor { get; private set; }
    public PostponedModelData(T model, Enum actionType, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null)
    {
        Model = model;
        Additional = additional;
        ActionType = actionType;
        OverrideLogin = overrideLogin;
        OverrideColor = overrideColor;
    }
}

public interface IPosponedActionsStorage<TAuditAction>
    where TAuditAction : class, IAuditAction
{
    IPostponedAuditManager<TAuditAction> PostponedAuditManager { get; }
    public IAuditActionFactory<TAuditAction> ActionFactory { get; }
    Task PlanPerformActionsAsync(DbContext context);
}
public sealed class PostponedAuditStorage<T, TAuditResult, TAuditAction> : IPosponedActionsStorage<TAuditAction>
    where T : class, IAuditable<TAuditResult, TAuditAction>
    where TAuditResult : class, IAuditResult<TAuditAction>
    where TAuditAction : class, IAuditAction
{
    private List<PostponedModelData<T>> _postponedModels;

    public IPostponedAuditManager<TAuditAction> PostponedAuditManager { get; private set; }
    public IAuditActionFactory<TAuditAction> ActionFactory => PostponedAuditManager.ActionFactory;

    public PostponedAuditStorage(IPostponedAuditManager<TAuditAction> postponedAuditManager)
    {
        PostponedAuditManager = postponedAuditManager;
        _postponedModels = new List<PostponedModelData<T>>();
    }

    #region Postpone
    public void Postpone(T model, Enum type, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null)
        => _postponedModels.Add(new PostponedModelData<T>(model, type, additional, overrideLogin, overrideColor));
    public void PostponeRange(IEnumerable<T> models, Enum type, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null)
        => _postponedModels.AddRange(models.Select(x => new PostponedModelData<T>(x, type, additional, overrideLogin, overrideColor)));
    #endregion

    #region PlanPerformActions
    private async Task PlanAddActionsAsync(DbContext context, List<TAuditResult> list)
    {
        if (_postponedModels.Count == 0)
        {
            return;
        }

        foreach (var modelData in _postponedModels)
        {
            var model = modelData.Model;
            TAuditResult action = await model.AuditAsync(context);
            string entityId = context.GetAuditEntityId(model);
            action.Action = ActionFactory.CreateAuditAction(modelData.ActionType, entityId, modelData.Additional, modelData.OverrideLogin, modelData.OverrideColor);
            list.Add(action);
        }
    }
    public async Task PlanPerformActionsAsync(DbContext context)
    {
        List<TAuditResult> actionAddList = new List<TAuditResult>();
        await PlanAddActionsAsync(context, actionAddList);
        await context.AddRangeAsync(actionAddList);
    }
    #endregion
}