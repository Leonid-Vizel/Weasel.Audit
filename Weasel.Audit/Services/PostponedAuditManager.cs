using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Weasel.Audit.Enums;
using Weasel.Audit.Interfaces;

namespace Weasel.Audit.Services;
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
    PostponedAuditStorage<T, TAudit> GetOrAddStorage<T, TAudit>()
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity;
    Task<TAudit> PlanAknowledgeAsync<T, TAudit>(DbContext context, T model)
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity;
    void PostponeCreate<T, TAudit>(T model, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity;
    void PostponeCreateRange<T, TAudit>(IEnumerable<T> models, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity;
    void PostponeUpdate<T, TAudit>(T model, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity;
    void PostponeUpdateRange<T, TAudit>(IEnumerable<T> models, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity;
    void PostponeDelete<T, TAudit>(T model, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity;
    void PostponeDeleteRange<T, TAudit>(IEnumerable<T> models, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity;
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

    public PostponedAuditStorage<T, TAudit> GetOrAddStorage<T, TAudit>()
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity
    {
        var key = new PostponedAuditStorageKey(typeof(T), typeof(TAudit));
        if (!_storages.TryGetValue(key, out var storage))
        {
            storage = new PostponedAuditStorage<T, TAudit>(this);
            _storages.Add(key, storage);
        }
        return (PostponedAuditStorage<T, TAudit>)storage;
    }

    #region Aknowledge
    public async Task<TAudit> PlanAknowledgeAsync<T, TAudit>(DbContext context, T model)
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity
        => await GetOrAddStorage<T, TAudit>().PostponeAnknowledgeAction(context, model);
    #endregion

    #region Create
    public void PostponeCreate<T, TAudit>(T model, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity
        => GetOrAddStorage<T, TAudit>().PostponeCreate(model, userId, SchemeManager.GetFirstSchemaAuditType<TAudit>(type, AuditScheme.Create), overrideLogin, overrideColor);
    public void PostponeCreateRange<T, TAudit>(IEnumerable<T> models, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity
        => GetOrAddStorage<T, TAudit>().PostponeCreateRange(models, userId, SchemeManager.GetFirstSchemaAuditType<TAudit>(type, AuditScheme.Create), overrideLogin, overrideColor);
    #endregion

    #region Update
    public void PostponeUpdate<T, TAudit>(T model, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity
        => GetOrAddStorage<T, TAudit>().PostponeUpdate(model, userId, SchemeManager.GetFirstSchemaAuditType<TAudit>(type, AuditScheme.Update), overrideLogin, overrideColor);
    public void PostponeUpdateRange<T, TAudit>(IEnumerable<T> models, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity
        => GetOrAddStorage<T, TAudit>().PostponeUpdateRange(models, userId, SchemeManager.GetFirstSchemaAuditType<TAudit>(type, AuditScheme.Update), overrideLogin, overrideColor);
    #endregion

    #region Delete
    public void PostponeDelete<T, TAudit>(T model, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity
        => GetOrAddStorage<T, TAudit>().PostponeDelete(model, userId, SchemeManager.GetFirstSchemaAuditType<TAudit>(type, AuditScheme.Delete), overrideLogin, overrideColor);
    public void PostponeDeleteRange<T, TAudit>(IEnumerable<T> models, int? userId, Enum? type, string? overrideLogin = null, Enum? overrideColor = null)
        where T : class, IIntKeyedEntity, IAuditable<TAudit>
        where TAudit : class, IIntKeyedEntity
        => GetOrAddStorage<T, TAudit>().PostponeDeleteRange(models, userId, SchemeManager.GetFirstSchemaAuditType<TAudit>(type, AuditScheme.Delete), overrideLogin, overrideColor);
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
                StateManager.PushStates();
                List<IAuditAction> dataActions = new List<IAuditAction>();
                foreach (var storage in _storages.Values)
                {
                    storage.LoadDataActions(dataActions);
                }
                await context.AddRangeAsync(dataActions);
                await context.SaveChangesAsync();
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
