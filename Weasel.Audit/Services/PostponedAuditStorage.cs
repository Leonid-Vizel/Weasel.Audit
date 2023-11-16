using Microsoft.EntityFrameworkCore;
using Weasel.Audit.Enums;
using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Services;

public struct PostponedModelData<T> where T : class
{
    public T Model { get; private set; }
    public int? UserId { get; private set; }
    public Enum ActionType { get; private set; }
    public string? OverrideLogin { get; private set; }
    public Enum? OverrideColor { get; private set; }
    public PostponedModelData(T model, int? userId, Enum actionType, string? overrideLogin, Enum? overrideColor)
    {
        Model = model;
        UserId = userId;
        ActionType = actionType;
        OverrideLogin = overrideLogin;
        OverrideColor = overrideColor;
    }
}
public struct PostponedInfoActionData<TAudit> where TAudit : class
{
    public int EntityId { get; private set; }
    public TAudit Action { get; private set; }
    public int? UserId { get; private set; }
    public Enum ActionType { get; private set; }
    public string? OverrideLogin { get; private set; }
    public Enum? OverrideColor { get; private set; }
    public PostponedInfoActionData(int entityId, TAudit action, int? userId, Enum actionType, string? overrideLogin, Enum? overrideColor)
    {
        EntityId = entityId;
        Action = action;
        UserId = userId;
        ActionType = actionType;
        OverrideLogin = overrideLogin;
        OverrideColor = overrideColor;
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
    public Enum? OverrideColor { get; private set; }
    public PostponedUpdateActionData(int entityId, TAudit oldAction, TAudit newAction, int? userId, Enum actionType, string? overrideLogin, Enum? overrideColor)
    {
        EntityId = entityId;
        OldAction = oldAction;
        NewAction = newAction;
        UserId = userId;
        ActionType = actionType;
        OverrideLogin = overrideLogin;
        OverrideColor = overrideColor;
    }
}

public interface IPosponedActionsStorage
{
    IPostponedAuditManager PostponedAuditManager { get; }
    public IAuditSchemeManager SchemeManager { get; }
    public IAuditActionFactory ActionFactory { get; }
    public IAuditStateManager StateManager { get; }
    Task PlanPerformActionsAsync(DbContext context);
    void LoadDataActions(List<IAuditAction> list);
}
public sealed class PostponedAuditStorage<T, TAudit> : IPosponedActionsStorage
    where T : class, IIntKeyedEntity, IAuditable<TAudit>
    where TAudit : class, IIntKeyedEntity
{
    private List<PostponedModelData<T>> _createModels;
    private List<PostponedModelData<T>> _updateModels;
    private List<PostponedModelData<T>> _deleteModels;

    private List<PostponedInfoActionData<TAudit>> _aknowledgeActions;
    private List<PostponedInfoActionData<TAudit>> _createActions;
    private List<PostponedUpdateActionData<TAudit>> _updateActions;
    private List<PostponedInfoActionData<TAudit>> _deleteActions;

    public IPostponedAuditManager PostponedAuditManager { get; private set; }
    public IAuditSchemeManager SchemeManager => PostponedAuditManager.SchemeManager;
    public IAuditActionFactory ActionFactory => PostponedAuditManager.ActionFactory;
    public IAuditStateManager StateManager => PostponedAuditManager.StateManager;

    public PostponedAuditStorage(IPostponedAuditManager postponedAuditManager)
    {
        PostponedAuditManager = postponedAuditManager;

        _createModels = new List<PostponedModelData<T>>();
        _updateModels = new List<PostponedModelData<T>>();
        _deleteModels = new List<PostponedModelData<T>>();

        _aknowledgeActions = new List<PostponedInfoActionData<TAudit>>();
        _createActions = new List<PostponedInfoActionData<TAudit>>();
        _updateActions = new List<PostponedUpdateActionData<TAudit>>();
        _deleteActions = new List<PostponedInfoActionData<TAudit>>();
    }

    #region Anknowledge
    public async Task<TAudit> PostponeAnknowledgeAction(DbContext context, T model)
    {
        TAudit audit = await model.AuditAsync(context, PostponedAuditManager);
        Enum actionType = SchemeManager.GetFirstSchemaAuditType<TAudit>(null, AuditScheme.Aknowledge);
        _aknowledgeActions.Add(new PostponedInfoActionData<TAudit>(model.Id, audit, null, actionType, "AKNOWLEDGE", null));
        return audit;
    }
    #endregion

    #region Create
    public void PostponeCreate(T model, int? userId, Enum type, string? overrideLogin = null, Enum? overrideColor = null)
        => _createModels.Add(new PostponedModelData<T>(model, userId, type, overrideLogin, overrideColor));
    public void PostponeCreateRange(IEnumerable<T> models, int? userId, Enum type, string? overrideLogin = null, Enum? overrideColor = null)
        => _createModels.AddRange(models.Select(x => new PostponedModelData<T>(x, userId, type, overrideLogin, overrideColor)));
    #endregion

    #region Update
    public void PostponeUpdate(T model, int? userId, Enum type, string? overrideLogin = null, Enum? overrideColor = null)
        => _updateModels.Add(new PostponedModelData<T>(model, userId, type, overrideLogin, overrideColor));
    public void PostponeUpdateRange(IEnumerable<T> models, int? userId, Enum type, string? overrideLogin = null, Enum? overrideColor = null)
        => _updateModels.AddRange(models.Select(x => new PostponedModelData<T>(x, userId, type, overrideLogin, overrideColor)));
    #endregion

    #region Delete
    public void PostponeDelete(T model, int? userId, Enum type, string? overrideLogin = null, Enum? overrideColor = null)
        => _deleteModels.Add(new PostponedModelData<T>(model, userId, type, overrideLogin, overrideColor));
    public void PostponeDeleteRange(IEnumerable<T> models, int? userId, Enum type, string? overrideLogin = null, Enum? overrideColor = null)
        => _deleteModels.AddRange(models.Select(x => new PostponedModelData<T>(x, userId, type, overrideLogin, overrideColor)));
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
            TAudit action = await modelData.Model.AuditAsync(context, PostponedAuditManager);
            list.Add(action);
            _createActions.Add(new PostponedInfoActionData<TAudit>(modelData.Model.Id, action, modelData.UserId, modelData.ActionType, modelData.OverrideLogin, modelData.OverrideColor));
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
            TAudit oldAction = await StateManager.GetLastAction<T, TAudit>(context, modelData.Model);
            TAudit newAction = await modelData.Model.AuditAsync(context, PostponedAuditManager);
            list.Add(newAction);
            StateManager.CommitState(modelData.Model.Id, newAction);
            _updateActions.Add(new PostponedUpdateActionData<TAudit>(modelData.Model.Id, oldAction, newAction, modelData.UserId, modelData.ActionType, modelData.OverrideLogin, modelData.OverrideColor));
        }
    }
    private async Task PlanDeleteActionsAsync(DbContext context)
    {
        if (_deleteModels.Count == 0)
        {
            return;
        }

        foreach (var modelData in _deleteModels)
        {
            TAudit action = await StateManager.GetLastAction<T, TAudit>(context, modelData.Model);
            _deleteActions.Add(new PostponedInfoActionData<TAudit>(modelData.Model.Id, action, modelData.UserId, modelData.ActionType, modelData.OverrideLogin, modelData.OverrideColor));
        }
    }
    public async Task PlanPerformActionsAsync(DbContext context)
    {
        List<TAudit> actionAddList = new List<TAudit>();

        await PlanAddActionsAsync(context, actionAddList);
        await PlanUpdateActionsAsync(context, actionAddList);
        await PlanDeleteActionsAsync(context);

        await context.AddRangeAsync(actionAddList);
    }
    #endregion

    #region PlanDataActions
    private void AddAknowledgeDataActions(List<IAuditAction> list)
    {
        if (_aknowledgeActions.Count == 0)
        {
            return;
        }
        foreach (var modelData in _aknowledgeActions)
        {
            var resultAction = ActionFactory.CreateIntAuditAction(modelData.ActionType, modelData.EntityId, modelData.UserId, modelData.Action.Id, null, modelData.OverrideLogin, modelData.OverrideColor, DateTime.Now);
            list.Add(resultAction);
        }
    }
    private void AddCreateDataActions(List<IAuditAction> list)
    {
        if (_createActions.Count == 0)
        {
            return;
        }
        foreach (var modelData in _createActions)
        {
            var resultAction = ActionFactory.CreateIntAuditAction(modelData.ActionType, modelData.EntityId, modelData.UserId, modelData.Action.Id, null, modelData.OverrideLogin, modelData.OverrideColor, DateTime.Now);
            list.Add(resultAction);
        }
    }
    private void AddUpdateDataActions(List<IAuditAction> list)
    {
        if (_updateActions.Count == 0)
        {
            return;
        }
        foreach (var modelData in _updateActions)
        {
            var resultAction = ActionFactory.CreateIntAuditAction(modelData.ActionType, modelData.EntityId, modelData.UserId, modelData.NewAction.Id, modelData.OldAction.Id, modelData.OverrideLogin, null, DateTime.Now);
            list.Add(resultAction);
        }
    }
    private void AddDeleteDataActions(List<IAuditAction> list)
    {
        if (_deleteActions.Count == 0)
        {
            return;
        }
        foreach (var modelData in _deleteActions)
        {
            var resultAction = ActionFactory.CreateIntAuditAction(modelData.ActionType, modelData.EntityId, modelData.UserId, null, modelData.Action.Id, modelData.OverrideLogin, null, DateTime.Now);
            list.Add(resultAction);
        }
    }
    public void LoadDataActions(List<IAuditAction> list)
    {
        AddAknowledgeDataActions(list);
        AddCreateDataActions(list);
        AddUpdateDataActions(list);
        AddDeleteDataActions(list);
    }
    #endregion
}