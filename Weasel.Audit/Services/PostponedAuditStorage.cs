using Microsoft.EntityFrameworkCore;
using Weasel.Audit.Enums;
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
public struct PostponedInfoActionData<TAudit> where TAudit : class
{
    public string EntityId { get; private set; }
    public TAudit Action { get; private set; }
    public Enum ActionType { get; private set; }
    public object? Additional { get; private set; }
    public string? OverrideLogin { get; private set; }
    public Enum? OverrideColor { get; private set; }
    public PostponedInfoActionData(string entityId, TAudit action, Enum actionType, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null)
    {
        EntityId = entityId;
        Action = action;
        Additional = additional;
        ActionType = actionType;
        OverrideLogin = overrideLogin;
        OverrideColor = overrideColor;
    }

    public static PostponedInfoActionData<TAudit> Create<T>(string entityId, TAudit action, PostponedModelData<T> data) where T : class
        => new PostponedInfoActionData<TAudit>(entityId, action, data.ActionType, data.Additional, data.OverrideLogin, data.OverrideColor);
}
public struct PostponedUpdateActionData<TAudit> where TAudit : class
{
    public string EntityId { get; private set; }
    public TAudit OldAction { get; private set; }
    public TAudit UpdateAction { get; private set; }
    public Enum ActionType { get; private set; }
    public object? Additional { get; private set; }
    public string? OverrideLogin { get; private set; }
    public Enum? OverrideColor { get; private set; }
    public PostponedUpdateActionData(string entityId, TAudit oldAction, TAudit updateAction, Enum actionType, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null)
    {
        EntityId = entityId;
        OldAction = oldAction;
        UpdateAction = updateAction;
        Additional = additional;
        ActionType = actionType;
        OverrideLogin = overrideLogin;
        OverrideColor = overrideColor;
    }
    public static PostponedUpdateActionData<TAudit> Create<T>(string entityId, TAudit oldAction, TAudit updateAction, PostponedModelData<T> data) where T : class
        => new PostponedUpdateActionData<TAudit>(entityId, oldAction, updateAction, data.ActionType, data.Additional, data.OverrideLogin, data.OverrideColor);
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
    where T : class, IAuditable<TAudit>
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
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }
        TAudit audit = await model.AuditAsync(context);
        Enum actionType = SchemeManager.GetFirstSchemaAuditType<TAudit>(null, AuditScheme.Aknowledge);
        string entityId = context.GetAuditEntityId(model);
        _aknowledgeActions.Add(new PostponedInfoActionData<TAudit>(entityId, audit, actionType, null, "AKNOWLEDGE", null));
        return audit;
    }
    #endregion

    #region Create
    public void PostponeCreate(T model, Enum type, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null)
        => _createModels.Add(new PostponedModelData<T>(model, type, additional, overrideLogin, overrideColor));
    public void PostponeCreateRange(IEnumerable<T> models, Enum type, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null)
        => _createModels.AddRange(models.Select(x => new PostponedModelData<T>(x, type, additional, overrideLogin, overrideColor)));
    #endregion

    #region Update
    public void PostponeUpdate(T model, Enum type, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null)
        => _updateModels.Add(new PostponedModelData<T>(model, type, additional, overrideLogin, overrideColor));
    public void PostponeUpdateRange(IEnumerable<T> models, Enum type, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null)
        => _updateModels.AddRange(models.Select(x => new PostponedModelData<T>(x, type, additional, overrideLogin, overrideColor)));
    #endregion

    #region Delete
    public void PostponeDelete(T model, Enum type, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null)
        => _deleteModels.Add(new PostponedModelData<T>(model, type, additional, overrideLogin, overrideColor));
    public void PostponeDeleteRange(IEnumerable<T> models, Enum type, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null)
        => _deleteModels.AddRange(models.Select(x => new PostponedModelData<T>(x, type, additional, overrideLogin, overrideColor)));
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
            var model = modelData.Model;
            TAudit action = await model.AuditAsync(context);
            list.Add(action);
            string entityId = context.GetAuditEntityId(model);
            _createActions.Add(PostponedInfoActionData<TAudit>.Create(entityId, action, modelData));
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
            var model = modelData.Model;
            TAudit oldAction = await StateManager.GetLastAction<T, TAudit>(context, model);
            TAudit newAction = await model.AuditAsync(context);
            list.Add(newAction);
            var entityId = context.GetAuditEntityId(model);
            StateManager.CommitState(entityId, newAction);
            _updateActions.Add(PostponedUpdateActionData<TAudit>.Create(entityId, oldAction, newAction, modelData));
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
            var model = modelData.Model;
            TAudit action = await StateManager.GetLastAction<T, TAudit>(context, model);
            var entityId = context.GetAuditEntityId(model);
            _deleteActions.Add(PostponedInfoActionData<TAudit>.Create(entityId, action, modelData));
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
            var resultAction = ActionFactory.CreateAuditAction(modelData.ActionType, modelData.EntityId, modelData.Additional, modelData.Action.Id, null, modelData.OverrideLogin, modelData.OverrideColor);
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
            var resultAction = ActionFactory.CreateAuditAction(modelData.ActionType, modelData.EntityId, modelData.Additional, modelData.Action.Id, null, modelData.OverrideLogin, modelData.OverrideColor);
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
            var resultAction = ActionFactory.CreateAuditAction(modelData.ActionType, modelData.EntityId, modelData.Additional, modelData.UpdateAction.Id, modelData.OldAction.Id, modelData.OverrideLogin, modelData.OverrideColor);
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
            var resultAction = ActionFactory.CreateAuditAction(modelData.ActionType, modelData.EntityId, modelData.Additional, null, modelData.Action.Id, modelData.OverrideLogin, modelData.OverrideColor);
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