using ECRF.Tools.Actions.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weasel.Audit.Interfaces;
using Weasel.Enums;

namespace Weasel.Services.Audit;
public struct PostponedAuditStorageKey
{
    public Type Type { get; private set; }
    public Type ActionType { get; private set; }
    public PostponedAuditStorageKey(Type type, Type actionType)
    {
        Type = type;
        ActionType = actionType;
    }
}
public interface IPostponedAuditManager
{
    IServiceProvider ServiceProvider { get; }
    IAuditActionFactory ActionFactory { get; }
    IAuditStateManager StateManager { get; }
    IAuditSchemeManager SchemeManager { get; }
    IAuditStateStorage GlobalStateStorage { get; }
    PostponedAuditStorage<T, TAction> GetOrAddStorage<T, TAction>()
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity;
    Task<TAction> PlanAknowledgeAsync<T, TAction>(DbContext context, T model)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity;
    void PostponeCreate<T, TAction>(T model, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity;
    void PostponeCreateRange<T, TAction>(IEnumerable<T> models, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity;
    void PostponeUpdate<T, TAction>(T model, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity;
    void PostponeUpdateRange<T, TAction>(IEnumerable<T> models, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity;
    void PostponeDelete<T, TAction>(T model, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity;
    void PostponeDeleteRange<T, TAction>(IEnumerable<T> models, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity;
    Task ExecuteAndDispose();
}
public sealed class PostponedAuditManager<TContext> : IPostponedAuditManager where TContext : DbContext
{
    private Dictionary<PostponedAuditStorageKey, IPosponedActionsStorage> _storages;
    private ILogger<PostponedAuditManager<TContext>> _logger;
    public IServiceProvider ServiceProvider { get; private set; }
    public IAuditActionFactory ActionFactory { get; private set; }
    public IAuditStateManager StateManager { get; private set; }
    public IAuditSchemeManager SchemeManager { get; private set; }
    public IAuditStateStorage GlobalStateStorage { get; private set; }
    public PostponedAuditManager(IServiceProvider provider, ILoggerFactory loggerFactory, IAuditSchemeManager schemeManager, IAuditActionFactory actionFactory, IAuditStateStorage globalStateStorage)
    {
        ServiceProvider = provider;
        SchemeManager = schemeManager;
        ActionFactory = actionFactory;
        GlobalStateStorage = globalStateStorage;
        StateManager = new AuditStateManager(this);
        _logger = loggerFactory.CreateLogger<PostponedAuditManager<TContext>>();
        _storages = new Dictionary<PostponedAuditStorageKey, IPosponedActionsStorage>();
    }

    public PostponedAuditStorage<T, TAction> GetOrAddStorage<T, TAction>()
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity
    {
        var key = new PostponedAuditStorageKey(typeof(T), typeof(TAction));
        if (!_storages.TryGetValue(key, out var storage))
        {
            storage = new PostponedAuditStorage<T, TAction>(this);
            _storages.Add(key, storage);
        }
        return (PostponedAuditStorage<T, TAction>)storage;
    }

    #region Aknowledge
    public async Task<TAction> PlanAknowledgeAsync<T, TAction>(DbContext context, T model)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity
        => await GetOrAddStorage<T, TAction>().PostponeAnknowledgeAction(context, model);
    #endregion

    #region Create
    public void PostponeCreate<T, TAction>(T model, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity
        => GetOrAddStorage<T, TAction>().PostponeCreate(model, userId, SchemeManager.GetFirstSchemaAuditType<TAction>(type, AuditScheme.Create), overrideLogin, overrideColor);
    public void PostponeCreateRange<T, TAction>(IEnumerable<T> models, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity
        => GetOrAddStorage<T, TAction>().PostponeCreateRange(models, userId, SchemeManager.GetFirstSchemaAuditType<TAction>(type, AuditScheme.Create), overrideLogin, overrideColor);
    #endregion

    #region Update
    public void PostponeUpdate<T, TAction>(T model, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity
        => GetOrAddStorage<T, TAction>().PostponeUpdate(model, userId, SchemeManager.GetFirstSchemaAuditType<TAction>(type, AuditScheme.Update), overrideLogin, overrideColor);
    public void PostponeUpdateRange<T, TAction>(IEnumerable<T> models, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity
        => GetOrAddStorage<T, TAction>().PostponeUpdateRange(models, userId, SchemeManager.GetFirstSchemaAuditType<TAction>(type, AuditScheme.Update), overrideLogin, overrideColor);
    #endregion

    #region Delete
    public void PostponeDelete<T, TAction>(T model, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity
        => GetOrAddStorage<T, TAction>().PostponeDelete(model, userId, SchemeManager.GetFirstSchemaAuditType<TAction>(type, AuditScheme.Delete), overrideLogin, overrideColor);
    public void PostponeDeleteRange<T, TAction>(IEnumerable<T> models, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAction>
        where TAction : class, IIntKeyedEntity
        => GetOrAddStorage<T, TAction>().PostponeDeleteRange(models, userId, SchemeManager.GetFirstSchemaAuditType<TAction>(type, AuditScheme.Delete), overrideLogin, overrideColor);
    #endregion

    #region ExecutePostonedActions
    public async Task ExecuteAndDispose()
    {
        if (_storages.Count == 0)
        {
            return;
        }
        using (var scope = ServiceProvider.CreateScope())
        using (var context = scope.ServiceProvider.GetRequiredService<TContext>())
        {
            try
            {
                foreach (var storage in _storages.Values)
                {
                    await storage.PlanPerformActionsAsync(context);
                }
                await context.SaveChangesAsync();
                List<IAuditAction> dataActions = new List<IAuditAction>();
                foreach (var storage in _storages.Values)
                {
                    storage.LoadDataActions(dataActions);
                }
                await context.AddRangeAsync(dataActions);
                await context.SaveChangesAsync();
                StateManager.PushStates();
                _logger.LogInformation($"Performed {dataActions.Count} postponed action(s) for {_storages.Count} type(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
    }
    #endregion
}
