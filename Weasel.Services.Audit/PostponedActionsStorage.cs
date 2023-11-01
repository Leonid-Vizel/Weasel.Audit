using Microsoft.EntityFrameworkCore;

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
public struct PostponedInfoActionData<TAction> where TAction : class
{
    public int EntityId { get; private set; }
    public TAction Action { get; private set; }
    public int? UserId { get; private set; }
    public Enum ActionType { get; private set; }
    public string? OverrideLogin { get; private set; }
    public PostponedInfoActionData(int entityId, TAction action, int? userId, Enum actionType, string? overrideLogin)
    {
        EntityId = entityId;
        Action = action;
        UserId = userId;
        ActionType = actionType;
        OverrideLogin = overrideLogin;
    }
}
public struct PostponedUpdateActionData<TAction> where TAction : class
{
    public int EntityId { get; private set; }
    public TAction OldAction { get; private set; }
    public TAction NewAction { get; private set; }
    public int? UserId { get; private set; }
    public Enum ActionType { get; private set; }
    public string? OverrideLogin { get; private set; }
    public PostponedUpdateActionData(int entityId, TAction oldAction, TAction newAction, int? userId, Enum actionType, string? overrideLogin)
    {
        EntityId = entityId;
        OldAction = oldAction;
        NewAction = newAction;
        UserId = userId;
        ActionType = actionType;
        OverrideLogin = overrideLogin;
    }
}

public sealed class PostponedActionsStorage<T, TAction> : IPosponedActionsStorage
    where T : class, IAuditable<TAction>
    where TAction : class
{
    private List<PostponedModelData<T>> _createModels;
    private List<PostponedModelData<T>> _updateModels;
    private List<PostponedModelData<T>> _deleteModels;

    private List<PostponedInfoActionData<TAction>> _createActions;
    private List<PostponedUpdateActionData<TAction>> _updateActions;
    private List<PostponedInfoActionData<TAction>> _deleteActions;

    public PostponedActionsStorage()
    {
        _createModels = new List<PostponedModelData<T>>();
        _updateModels = new List<PostponedModelData<T>>();
        _deleteModels = new List<PostponedModelData<T>>();

        _createActions = new List<PostponedInfoActionData<TAction>>();
        _updateActions = new List<PostponedUpdateActionData<TAction>>();
        _deleteActions = new List<PostponedInfoActionData<TAction>>();
    }

    #region Types
    public Type GetEntityType()
        => typeof(T);
    public Type GetActionType()
        => typeof(TAction);
    #endregion

    #region Create
    public void PostponeCreate(T model, int? userId, DataActionType? type, string? overrideLogin = null, ActionColor? overrideColor = null)
        => _createModels.Add(new PostponedModelData<T>(model, userId, type, overrideLogin, overrideColor));

    public void PostponeCreateRange(IEnumerable<T> models, int? userId, DataActionType? type, string? overrideLogin = null, ActionColor? overrideColor = null)
        => _createModels.AddRange(models.Select(x => new PostponedModelData<T>(x, userId, type, overrideLogin, overrideColor)));
    #endregion

    #region Update
    public void PostponeUpdate(T model, int? userId, DataActionType? type, string? overrideLogin = null, ActionColor? overrideColor = null)
        => _updateModels.Add(new PostponedModelData<T>(model, userId, type, overrideLogin, overrideColor));

    public void PostponeUpdateRange(IEnumerable<T> models, int? userId, DataActionType? type, string? overrideLogin = null, ActionColor? overrideColor = null)
        => _updateModels.AddRange(models.Select(x => new PostponedModelData<T>(x, userId, type, overrideLogin, overrideColor)));
    #endregion

    #region Delete
    public void PostponeDelete(T model, int? userId, DataActionType? type, string? overrideLogin = null, ActionColor? overrideColor = null)
        => _deleteModels.Add(new PostponedModelData<T>(model, userId, type, overrideLogin, overrideColor));

    public void PostponeDeleteRange(IEnumerable<T> models, int? userId, DataActionType? type, string? overrideLogin = null, ActionColor? overrideColor = null)
        => _deleteModels.AddRange(models.Select(x => new PostponedModelData<T>(x, userId, type, overrideLogin, overrideColor)));
    #endregion

    #region PlanPerformActions
    private async Task PlanAddActionsAsync(DbContext context, List<TAction> list)
    {
        if (_createModels.Count == 0)
        {
            return;
        }

        foreach (var modelData in _createModels)
        {
            TAction action = await modelData.Model.Copy(context);
            list.Add(action);
            _createActions.Add(new PostponedInfoActionData<TAction>(modelData.Model.Id, action, modelData.UserId, modelData.ActionType, modelData.OverrideLogin, modelData.OverrideColor));
        }
    }

    private async Task PlanUpdateActionsAsync(DbContext context, List<TAction> list)
    {
        if (_updateModels.Count == 0)
        {
            return;
        }

        foreach (var modelData in _updateModels)
        {
            TAction? newAction = await modelData.Model.Copy(context);
            list.Add(newAction);
            TAction? oldAction = await ActionUtils.GetLastAction<TAction>(context, modelData.Model.Id) ?? newAction;
            _updateActions.Add(new PostponedUpdateActionData<TAction>(modelData.Model.Id, oldAction, newAction, modelData.UserId, modelData.ActionType, modelData.OverrideLogin, modelData.OverrideColor));
        }
    }

    private async Task PlanDeleteActionsAsync(DbContext context, List<TAction> list)
    {
        if (_deleteModels.Count == 0)
        {
            return;
        }

        foreach (var modelData in _deleteModels)
        {
            TAction? action = await ActionUtils.GetLastAction<TAction>(context, modelData.Model.Id);
            if (action == null)
            {
                action = await modelData.Model.Copy(context);
                list.Add(action);
            }
            _deleteActions.Add(new PostponedInfoActionData<TAction>(modelData.Model.Id, action, modelData.UserId, modelData.ActionType, modelData.OverrideLogin, modelData.OverrideColor));
        }
    }

    public async Task PlanPerformActionsAsync(DbContext context)
    {
        List<TAction> actionAddList = new List<TAction>();

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
        DataActionType type = ActionUtils.GetTypeSchemaAction<TAction>(ActionScheme.Create);
        foreach (var modelData in _createActions)
        {
            DataAction resultAction = new DataAction()
            {
                Type = modelData.ActionType ?? type,
                UserId = modelData.UserId,
                NewDataId = modelData.Action.Id,
                EntityId = modelData.EntityId,
                OverrideLogin = modelData.OverrideLogin,
                OverrideColor = modelData.OverrideColor,
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
        DataActionType type = ActionUtils.GetTypeSchemaAction<TAction>(ActionScheme.Edit);
        foreach (var modelData in _updateActions)
        {
            DataAction resultAction = new DataAction()
            {
                Type = modelData.ActionType ?? type,
                UserId = modelData.UserId,
                OldDataId = modelData.OldAction.Id,
                NewDataId = modelData.NewAction.Id,
                EntityId = modelData.EntityId,
                OverrideLogin = modelData.OverrideLogin,
                OverrideColor = modelData.OverrideColor,
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
        DataActionType type = ActionUtils.GetTypeSchemaAction<TAction>(ActionScheme.Delete);
        foreach (var modelData in _deleteActions)
        {
            DataAction resultAction = new DataAction()
            {
                Type = modelData.ActionType ?? type,
                UserId = modelData.UserId,
                OldDataId = modelData.Action.Id,
                EntityId = modelData.EntityId,
                OverrideLogin = modelData.OverrideLogin,
                OverrideColor = modelData.OverrideColor,
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
