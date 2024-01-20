using Microsoft.EntityFrameworkCore;
using Weasel.Audit.Extensions;
using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Services;

public struct PostponedModelData<T, TEnum>
    where TEnum : struct, Enum
    where T : class
{
    public IEnumerable<T> Models { get; private set; }
    public TEnum ActionType { get; private set; }
    public object? Additional { get; private set; }
    public PostponedModelData(IEnumerable<T> models, TEnum actionType, object? additional = null)
    {
        Models = models;
        Additional = additional;
        ActionType = actionType;
    }
}

public interface IPosponedActionsStorage<TAction, TRow, TEnum, TColor>
    where TAction : class, IAuditAction<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
    where TColor : struct, Enum
	where TEnum : struct, Enum
{
    IPostponedAuditManager<TAction, TRow, TEnum, TColor> PostponedAuditManager { get; }
    public IAuditActionFactory<TAction, TRow, TEnum> ActionFactory { get; }
    Task PlanPerformActionsAsync(DbContext context);
}

public sealed class PostponedAuditStorage<T, TResult, TAction, TRow, TEnum, TColor> : IPosponedActionsStorage<TAction, TRow, TEnum, TColor>
    where T : class, IAuditable<TResult, TAction, TRow, TEnum>
    where TResult : class, IAuditResult<TAction, TRow, TEnum>
    where TAction : class, IAuditAction<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
    where TColor : struct, Enum
	where TEnum : struct, Enum
{
    private readonly List<PostponedModelData<T, TEnum>> _postponedModels;

    public IPostponedAuditManager<TAction, TRow, TEnum, TColor> PostponedAuditManager { get; private set; }
    public IAuditActionFactory<TAction, TRow, TEnum> ActionFactory => PostponedAuditManager.ActionFactory;
    public IAuditRowFactory<TRow, TEnum> RowFactory => PostponedAuditManager.RowFactory;

    public PostponedAuditStorage(IPostponedAuditManager<TAction, TRow, TEnum, TColor> postponedAuditManager)
    {
        PostponedAuditManager = postponedAuditManager;
        _postponedModels = [];
    }

    #region Postpone
    public void Postpone(T model, TEnum type, object? additional = null)
        => _postponedModels.Add(new PostponedModelData<T, TEnum>([model], type, additional));
    public void PostponeRange(IEnumerable<T> models, TEnum type, object? additional = null)
        => _postponedModels.Add(new PostponedModelData<T, TEnum>(models, type, additional));
    #endregion

    #region PlanPerformActions
    private async Task PlanAddActionsAsync(DbContext context, List<TResult> list)
    {
        if (_postponedModels.Count == 0)
        {
            return;
        }

        foreach (var modelData in _postponedModels)
        {
            var row = RowFactory.CreateAuditRow(modelData.ActionType, modelData.Additional);
            foreach (var model in modelData.Models)
            {
                TResult action = await model.AuditAsync(context);
                string entityId = context.GetAuditEntityId(model);
                action.Action = ActionFactory.CreateAuditAction(row, entityId, modelData.Additional);
                list.Add(action);
            }
        }
    }
    public async Task PlanPerformActionsAsync(DbContext context)
    {
        List<TResult> actionAddList = [];
        await PlanAddActionsAsync(context, actionAddList);
        await context.AddRangeAsync(actionAddList);
    }
    #endregion
}