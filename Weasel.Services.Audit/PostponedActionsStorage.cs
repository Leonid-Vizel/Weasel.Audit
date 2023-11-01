using ECRF.Tools.Actions.Interfaces;
using Microsoft.EntityFrameworkCore;
using Weasel.Enums;

namespace Weasel.Services.Audit;

public interface IPosponedActionsStorage
{
    Type GetEntityType();
    Type GetActionType();
    Task PlanPerformActionsAsync(DbContext context);
    void LoadDataActions(List<DataAction> list);
}

public struct PostponedModelData<T> where T : class
{
    public T Model { get; private set; }
    public int? UserId { get; private set; }
    public Enum ActionType { get; private set; }
    public string? OverrideLogin { get; private set; }
    public PostponedModelData(T model, int? userId, Enum actionType, string? overrideLogin)
    {
        Model = model;
        UserId = userId;
        ActionType = actionType;
        OverrideLogin = overrideLogin;
    }
}
public struct PostponedInfoActionData<TAudit> where TAudit : class
{
    public int EntityId { get; private set; }
    public TAudit Action { get; private set; }
    public int? UserId { get; private set; }
    public Enum ActionType { get; private set; }
    public string? OverrideLogin { get; private set; }
    public PostponedInfoActionData(int entityId, TAudit action, int? userId, Enum actionType, string? overrideLogin)
    {
        EntityId = entityId;
        Action = action;
        UserId = userId;
        ActionType = actionType;
        OverrideLogin = overrideLogin;
    }
}
public struct PostponedUpdateActionData<TAudit> where TAudit : class
{
    public int EntityId { get; private set; }
    public TAudit OldAction { get; private set; }
    public TAudit NewAction { get; private set; }
    public int? UserId { get; private set; }
    public Enum ActionType { get; private set; }
    public string? OverrideLogin { get; private set; }
    public PostponedUpdateActionData(int entityId, TAudit oldAction, TAudit newAction, int? userId, Enum actionType, string? overrideLogin)
    {
        EntityId = entityId;
        OldAction = oldAction;
        NewAction = newAction;
        UserId = userId;
        ActionType = actionType;
        OverrideLogin = overrideLogin;
    }
}

public sealed class PostponedActionsStorage<T, TAudit> : IPosponedActionsStorage
    where T : class, IAuditable<TAudit>
    where TAudit : class
{
    private List<PostponedModelData<T>> _createModels;
    private List<PostponedModelData<T>> _updateModels;
    private List<PostponedModelData<T>> _deleteModels;

    private List<PostponedInfoActionData<TAudit>> _createActions;
    private List<PostponedUpdateActionData<TAudit>> _updateActions;
    private List<PostponedInfoActionData<TAudit>> _deleteActions;

    public PostponedActionsStorage()
    {
        _createModels = new List<PostponedModelData<T>>();
        _updateModels = new List<PostponedModelData<T>>();
        _deleteModels = new List<PostponedModelData<T>>();

        _createActions = new List<PostponedInfoActionData<TAudit>>();
        _updateActions = new List<PostponedUpdateActionData<TAudit>>();
        _deleteActions = new List<PostponedInfoActionData<TAudit>>();
    }

    #region Types
    public Type GetEntityType()
        => typeof(T);
    public Type GetActionType()
        => typeof(TAudit);
    #endregion

    #region Create
    public void PostponeCreate(T model, int? userId, Enum type, string? overrideLogin = null)
        => _createModels.Add(new PostponedModelData<T>(model, userId, type, overrideLogin));

    public void PostponeCreateRange(IEnumerable<T> models, int? userId, Enum type, string? overrideLogin = null)
        => _createModels.AddRange(models.Select(x => new PostponedModelData<T>(x, userId, type, overrideLogin)));
    #endregion

    #region Update
    public void PostponeUpdate(T model, int? userId, Enum type, string? overrideLogin = null)
        => _updateModels.Add(new PostponedModelData<T>(model, userId, type, overrideLogin));

    public void PostponeUpdateRange(IEnumerable<T> models, int? userId, Enum type, string? overrideLogin = null)
        => _updateModels.AddRange(models.Select(x => new PostponedModelData<T>(x, userId, type, overrideLogin)));
    #endregion

    #region Delete
    public void PostponeDelete(T model, int? userId, Enum type, string? overrideLogin = null)
        => _deleteModels.Add(new PostponedModelData<T>(model, userId, type, overrideLogin));

    public void PostponeDeleteRange(IEnumerable<T> models, int? userId, Enum type, string? overrideLogin = null)
        => _deleteModels.AddRange(models.Select(x => new PostponedModelData<T>(x, userId, type, overrideLogin)));
    #endregion

    #region PlanPerformActions
    private async Task PlanAddActionsAsync(DbContext context, List<TAudit> list)
    {
        if (_createModels.Count == 0)
        {
            return;
        }

        foreach (var modelData in _createModels)
        {
            TAudit action = await modelData.Model.AuditAsync(context);
            list.Add(action);
            _createActions.Add(new PostponedInfoActionData<TAudit>(modelData.Model.Id, action, modelData.UserId, modelData.ActionType, modelData.OverrideLogin));
        }
    }

    private async Task PlanUpdateActionsAsync(DbContext context, List<TAudit> list)
    {
        if (_updateModels.Count == 0)
        {
            return;
        }

        foreach (var modelData in _updateModels)
        {
            TAudit? newAction = await modelData.Model.AuditAsync(context);
            list.Add(newAction);
            TAudit? oldAction = await ActionUtils.GetLastAction<TAudit>(context, modelData.Model.Id) ?? newAction;
            _updateActions.Add(new PostponedUpdateActionData<TAudit>(modelData.Model.Id, oldAction, newAction, modelData.UserId, modelData.ActionType, modelData.OverrideLogin));
        }
    }

    private async Task PlanDeleteActionsAsync(DbContext context, List<TAudit> list)
    {
        if (_deleteModels.Count == 0)
        {
            return;
        }

        foreach (var modelData in _deleteModels)
        {
            TAudit? action = await ActionUtils.GetLastAction<TAudit>(context, modelData.Model.Id);
            if (action == null)
            {
                action = await modelData.Model.AuditAsync(context);
                list.Add(action);
            }
            _deleteActions.Add(new PostponedInfoActionData<TAudit>(modelData.Model.Id, action, modelData.UserId, modelData.ActionType, modelData.OverrideLogin));
        }
    }

    public async Task PlanPerformActionsAsync(DbContext context)
    {
        List<TAudit> actionAddList = new List<TAudit>();

        await PlanAddActionsAsync(context, actionAddList);
        await PlanUpdateActionsAsync(context, actionAddList);
        await PlanDeleteActionsAsync(context, actionAddList);

        await context.AddRangeAsync(actionAddList);
    }
    #endregion

    #region PlanDataActions
    private void AddCreateDataActions(List<DataAction> list)
    {
        if (_createActions.Count == 0)
        {
            return;
        }
        Enum actionType = ActionUtils.GetTypeSchemaAction<TAudit>(AuditScheme.Create);
        foreach (var modelData in _createActions)
        {
            DataAction resultAction = new DataAction()
            {
                Type = modelData.ActionType ?? actionType,
                UserId = modelData.UserId,
                NewDataId = modelData.Action.Id,
                EntityId = modelData.EntityId,
                OverrideLogin = modelData.OverrideLogin,
            };
            list.Add(resultAction);
        }
    }
    private void AddUpdateDataActions(List<DataAction> list)
    {
        if (_updateActions.Count == 0)
        {
            return;
        }
        Enum actionType = ActionUtils.GetTypeSchemaAction<TAudit>(AuditScheme.Update);
        foreach (var modelData in _updateActions)
        {
            DataAction resultAction = new DataAction()
            {
                Type = modelData.ActionType ?? actionType,
                UserId = modelData.UserId,
                OldDataId = modelData.OldAction.Id,
                NewDataId = modelData.NewAction.Id,
                EntityId = modelData.EntityId,
                OverrideLogin = modelData.OverrideLogin,
            };
            list.Add(resultAction);
        }
    }
    private void AddDeleteDataActions(List<DataAction> list)
    {
        if (_deleteActions.Count == 0)
        {
            return;
        }
        Enum actionType = ActionUtils.GetTypeSchemaAction<TAudit>(AuditScheme.Delete);
        foreach (var modelData in _deleteActions)
        {
            DataAction resultAction = new DataAction()
            {
                Type = modelData.ActionType ?? actionType,
                UserId = modelData.UserId,
                OldDataId = modelData.Action.Id,
                EntityId = modelData.EntityId,
                OverrideLogin = modelData.OverrideLogin,
            };
            list.Add(resultAction);
        }
    }
    public void LoadDataActions(List<DataAction> list)
    {
        AddCreateDataActions(list);
        AddUpdateDataActions(list);
        AddDeleteDataActions(list);
    }
    #endregion
}
