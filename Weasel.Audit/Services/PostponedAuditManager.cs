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
public interface IPostponedAuditManager<TAuditAction>
     where TAuditAction : class, IAuditAction
{
    IServiceProvider ServiceProvider { get; }
    IAuditActionFactory<TAuditAction> ActionFactory { get; }
    IAuditSchemeManager SchemeManager { get; }
    PostponedAuditStorage<T, TAuditResult, TAuditAction> GetOrAddStorage<T, TAuditResult>()
        where T : class, IAuditable<TAuditResult, TAuditAction>
        where TAuditResult : class, IAuditResult<TAuditAction>;
    void PostponeCreate<T, TAuditResult>(T model, Enum? type = null, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction>
        where TAuditResult : class, IAuditResult<TAuditAction>;
    void PostponeCreateRange<T, TAuditResult>(IEnumerable<T> models, Enum? type = null, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction>
        where TAuditResult : class, IAuditResult<TAuditAction>;
    void PostponeUpdate<T, TAuditResult>(T model, Enum? type = null, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction>
        where TAuditResult : class, IAuditResult<TAuditAction>;
    void PostponeUpdateRange<T, TAuditResult>(IEnumerable<T> models, Enum? type = null, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction>
        where TAuditResult : class, IAuditResult<TAuditAction>;
    void PostponeDelete<T, TAuditResult>(T model, Enum? type = null, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction>
        where TAuditResult : class, IAuditResult<TAuditAction>;
    void PostponeDeleteRange<T, TAuditResult>(IEnumerable<T> models, Enum? type = null, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction>
        where TAuditResult : class, IAuditResult<TAuditAction>;
    Task ExecuteAndDispose();
}
public sealed class PostponedAuditManager<TContext, TAuditAction> : IPostponedAuditManager<TAuditAction>
    where TContext : DbContext
    where TAuditAction : class, IAuditAction
{
    private Dictionary<PostponedAuditStorageKey, IPosponedActionsStorage<TAuditAction>> _storages;
    private ILogger<PostponedAuditManager<TContext, TAuditAction>> _logger;
    public IServiceProvider ServiceProvider { get; private set; }
    public IAuditActionFactory<TAuditAction> ActionFactory { get; private set; }
    public IAuditSchemeManager SchemeManager { get; private set; }
    public PostponedAuditManager(IServiceProvider provider, ILoggerFactory loggerFactory, IAuditSchemeManager schemeManager, IAuditActionFactory<TAuditAction> actionFactory)
    {
        ServiceProvider = provider;
        SchemeManager = schemeManager;
        ActionFactory = actionFactory;
        _logger = loggerFactory.CreateLogger<PostponedAuditManager<TContext, TAuditAction>>();
        _storages = new Dictionary<PostponedAuditStorageKey, IPosponedActionsStorage<TAuditAction>>();
    }

    public PostponedAuditStorage<T, TAuditResult, TAuditAction> GetOrAddStorage<T, TAuditResult>()
        where T : class, IAuditable<TAuditResult, TAuditAction>
        where TAuditResult : class, IAuditResult<TAuditAction>
    {
        var key = new PostponedAuditStorageKey(typeof(T), typeof(TAuditResult));
        if (!_storages.TryGetValue(key, out var storage))
        {
            storage = new PostponedAuditStorage<T, TAuditResult, TAuditAction>(this);
            _storages.Add(key, storage);
        }
        return (PostponedAuditStorage<T, TAuditResult, TAuditAction>)storage;
    }

    #region Create
    public void PostponeCreate<T, TAuditResult>(T model, Enum? type, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction>
        where TAuditResult : class, IAuditResult<TAuditAction>
    {
        var storage = GetOrAddStorage<T, TAuditResult>();
        var scheme = custom ? AuditScheme.CustomCreate : AuditScheme.Create;
        type = type ?? SchemeManager.GetFirstSchemaAuditType<TAuditResult>(type, scheme);
        storage.Postpone(model, type, additional, overrideLogin, overrideColor);
    }
    public void PostponeCreateRange<T, TAuditResult>(IEnumerable<T> models, Enum? type, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction>
        where TAuditResult : class, IAuditResult<TAuditAction>
    {
        var storage = GetOrAddStorage<T, TAuditResult>();
        var scheme = custom ? AuditScheme.CustomCreate : AuditScheme.Create;
        type = type ?? SchemeManager.GetFirstSchemaAuditType<TAuditResult>(type, scheme);
        storage.PostponeRange(models, type, additional, overrideLogin, overrideColor);
    }
    #endregion

    #region Update
    public void PostponeUpdate<T, TAuditResult>(T model, Enum? type = null, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction>
        where TAuditResult : class, IAuditResult<TAuditAction>
    {
        var storage = GetOrAddStorage<T, TAuditResult>();
        var scheme = custom ? AuditScheme.CustomUpdate : AuditScheme.Update;
        type = type ?? SchemeManager.GetFirstSchemaAuditType<TAuditResult>(type, scheme);
        storage.Postpone(model, type, additional, overrideLogin, overrideColor);
    }
    public void PostponeUpdateRange<T, TAuditResult>(IEnumerable<T> models, Enum? type = null, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction>
        where TAuditResult : class, IAuditResult<TAuditAction>
    {
        var storage = GetOrAddStorage<T, TAuditResult>();
        var scheme = custom ? AuditScheme.CustomUpdate : AuditScheme.Update;
        type = type ?? SchemeManager.GetFirstSchemaAuditType<TAuditResult>(type, scheme);
        storage.PostponeRange(models, type, additional, overrideLogin, overrideColor);
    }
    #endregion

    #region Delete
    public void PostponeDelete<T, TAuditResult>(T model, Enum? type = null, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction>
        where TAuditResult : class, IAuditResult<TAuditAction>
    {
        var storage = GetOrAddStorage<T, TAuditResult>();
        var scheme = custom ? AuditScheme.CustomDelete : AuditScheme.Delete;
        type = type ?? SchemeManager.GetFirstSchemaAuditType<TAuditResult>(type, scheme);
        storage.Postpone(model, type, additional, overrideLogin, overrideColor);
    }
    public void PostponeDeleteRange<T, TAuditResult>(IEnumerable<T> models, Enum? type = null, object? additional = null, string? overrideLogin = null, Enum? overrideColor = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction>
        where TAuditResult : class, IAuditResult<TAuditAction>
    {
        var storage = GetOrAddStorage<T, TAuditResult>();
        var scheme = custom ? AuditScheme.CustomDelete : AuditScheme.Delete;
        type = type ?? SchemeManager.GetFirstSchemaAuditType<TAuditResult>(type, scheme);
        storage.PostponeRange(models, type, additional, overrideLogin, overrideColor);
    }
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
                _logger.LogInformation($"Performed postponed action(s) for types: {string.Join(", ", _storages.Select(x => x.Key.Type.Name))}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
    }
    #endregion
}
