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
public interface IPostponedAuditManager<TAction, TRow, TEnum, TColor>
    where TAction : class, IAuditAction<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
    where TColor : struct, Enum
    where TEnum : struct, Enum
{
    IServiceProvider ServiceProvider { get; }
    IAuditActionFactory<TAction, TRow, TEnum> ActionFactory { get; }
    IAuditRowFactory<TRow, TEnum> RowFactory { get; }
    IAuditSchemeManager<TEnum, TColor> SchemeManager { get; }
    PostponedAuditStorage<T, TResult, TAction, TRow, TEnum, TColor> GetOrAddStorage<T, TResult>()
        where T : class, IAuditable<TResult, TAction, TRow, TEnum>
        where TResult : class, IAuditResult<TAction, TRow, TEnum>;
    void PostponeCreate<T, TResult>(T model, TEnum? type = null, object? additional = null, bool custom = false)
        where T : class, IAuditable<TResult, TAction, TRow, TEnum>
        where TResult : class, IAuditResult<TAction, TRow, TEnum>;
    void PostponeCreateRange<T, TResult>(IEnumerable<T> models, TEnum? type = null, object? additional = null, bool custom = false, bool grouped = false)
        where T : class, IAuditable<TResult, TAction, TRow, TEnum>
        where TResult : class, IAuditResult<TAction, TRow, TEnum>;
    void PostponeUpdate<T, TResult>(T model, TEnum? type = null, object? additional = null, bool custom = false)
        where T : class, IAuditable<TResult, TAction, TRow, TEnum>
        where TResult : class, IAuditResult<TAction, TRow, TEnum>;
    void PostponeUpdateRange<T, TResult>(IEnumerable<T> models, TEnum? type = null, object? additional = null, bool custom = false, bool grouped = false)
        where T : class, IAuditable<TResult, TAction, TRow, TEnum>
        where TResult : class, IAuditResult<TAction, TRow, TEnum>;
    void PostponeDelete<T, TResult>(T model, TEnum? type = null, object? additional = null, bool custom = false)
        where T : class, IAuditable<TResult, TAction, TRow, TEnum>
        where TResult : class, IAuditResult<TAction, TRow, TEnum>;
    void PostponeDeleteRange<T, TResult>(IEnumerable<T> models, TEnum? type = null, object? additional = null, bool custom = false, bool grouped = false)
        where T : class, IAuditable<TResult, TAction, TRow, TEnum>
        where TResult : class, IAuditResult<TAction, TRow, TEnum>;
    Task ExecuteAndDispose();
}
public sealed class PostponedAuditManager<TContext, TAction, TRow, TEnum, TColor> : IPostponedAuditManager<TAction, TRow, TEnum, TColor>
    where TAction : class, IAuditAction<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
    where TColor : struct, Enum
    where TEnum : struct, Enum
    where TContext : DbContext
{
    private readonly Dictionary<PostponedAuditStorageKey, IPosponedActionsStorage<TAction, TRow, TEnum, TColor>> _storages;
    private readonly ILogger<PostponedAuditManager<TContext, TAction, TRow, TEnum, TColor>> _logger;
    public IServiceProvider ServiceProvider { get; private set; }
    public IAuditActionFactory<TAction, TRow, TEnum> ActionFactory { get; private set; }
    public IAuditRowFactory<TRow, TEnum> RowFactory { get; private set; }
    public IAuditSchemeManager<TEnum, TColor> SchemeManager { get; private set; }
    public PostponedAuditManager(IServiceProvider provider,
        ILoggerFactory loggerFactory,
        IAuditSchemeManager<TEnum, TColor> schemeManager,
        IAuditActionFactory<TAction, TRow, TEnum> actionFactory,
        IAuditRowFactory<TRow, TEnum> rowFactory)
    {
        ServiceProvider = provider;
        SchemeManager = schemeManager;
        ActionFactory = actionFactory;
        RowFactory = rowFactory;
        _logger = loggerFactory.CreateLogger<PostponedAuditManager<TContext, TAction, TRow, TEnum, TColor>>();
        _storages = [];
    }

    public PostponedAuditStorage<T, TResult, TAction, TRow, TEnum, TColor> GetOrAddStorage<T, TResult>()
        where T : class, IAuditable<TResult, TAction, TRow, TEnum>
        where TResult : class, IAuditResult<TAction, TRow, TEnum>
    {
        var key = new PostponedAuditStorageKey(typeof(T), typeof(TResult));
        if (!_storages.TryGetValue(key, out var storage))
        {
            storage = new PostponedAuditStorage<T, TResult, TAction, TRow, TEnum, TColor>(this);
            _storages.Add(key, storage);
        }
        return (PostponedAuditStorage<T, TResult, TAction, TRow, TEnum, TColor>)storage;
    }

    #region Create
    public void PostponeCreate<T, TResult>(T model, TEnum? type, object? additional = null, bool custom = false)
        where T : class, IAuditable<TResult, TAction, TRow, TEnum>
        where TResult : class, IAuditResult<TAction, TRow, TEnum>
    {
        var storage = GetOrAddStorage<T, TResult>();
        var scheme = custom ? AuditScheme.CustomCreate : AuditScheme.Create;
        type ??= SchemeManager.GetFirstSchemaAuditType<TResult>(type, scheme);
        storage.Postpone(model, type.Value, additional);
    }
    public void PostponeCreateRange<T, TResult>(IEnumerable<T> models, TEnum? type, object? additional = null, bool custom = false, bool grouped = false)
        where T : class, IAuditable<TResult, TAction, TRow, TEnum>
        where TResult : class, IAuditResult<TAction, TRow, TEnum>
    {
        var storage = GetOrAddStorage<T, TResult>();
        var scheme = custom ? AuditScheme.CustomCreate : AuditScheme.Create;
        type ??= SchemeManager.GetFirstSchemaAuditType<TResult>(type, scheme);
        if (grouped)
        {
            storage.PostponeRange(models, type.Value, additional);
        }
        else
        {
            foreach (var model in models)
            {
                storage.PostponeRange([model], type.Value, additional);
            }
        }
    }
    #endregion

    #region Update
    public void PostponeUpdate<T, TResult>(T model, TEnum? type = null, object? additional = null, bool custom = false)
        where T : class, IAuditable<TResult, TAction, TRow, TEnum>
        where TResult : class, IAuditResult<TAction, TRow, TEnum>
    {
        var storage = GetOrAddStorage<T, TResult>();
        var scheme = custom ? AuditScheme.CustomUpdate : AuditScheme.Update;
        type ??= SchemeManager.GetFirstSchemaAuditType<TResult>(type, scheme);
        storage.Postpone(model, type.Value, additional);
    }
    public void PostponeUpdateRange<T, TResult>(IEnumerable<T> models, TEnum? type = null, object? additional = null, bool custom = false, bool grouped = false)
        where T : class, IAuditable<TResult, TAction, TRow, TEnum>
        where TResult : class, IAuditResult<TAction, TRow, TEnum>
    {
        var storage = GetOrAddStorage<T, TResult>();
        var scheme = custom ? AuditScheme.CustomUpdate : AuditScheme.Update;
        type ??= SchemeManager.GetFirstSchemaAuditType<TResult>(type, scheme);
        if (grouped)
        {
            storage.PostponeRange(models, type.Value, additional);
        }
        else
        {
            foreach (var model in models)
            {
                storage.PostponeRange([model], type.Value, additional);
            }
        }
    }
    #endregion

    #region Delete
    public void PostponeDelete<T, TResult>(T model, TEnum? type = null, object? additional = null, bool custom = false)
        where T : class, IAuditable<TResult, TAction, TRow, TEnum>
        where TResult : class, IAuditResult<TAction, TRow, TEnum>
    {
        var storage = GetOrAddStorage<T, TResult>();
        var scheme = custom ? AuditScheme.CustomDelete : AuditScheme.Delete;
        type ??= SchemeManager.GetFirstSchemaAuditType<TResult>(type, scheme);
        storage.Postpone(model, type.Value, additional);
    }
    public void PostponeDeleteRange<T, TResult>(IEnumerable<T> models, TEnum? type = null, object? additional = null, bool custom = false, bool grouped = false)
        where T : class, IAuditable<TResult, TAction, TRow, TEnum>
        where TResult : class, IAuditResult<TAction, TRow, TEnum>
    {
        var storage = GetOrAddStorage<T, TResult>();
        var scheme = custom ? AuditScheme.CustomDelete : AuditScheme.Delete;
        type ??= SchemeManager.GetFirstSchemaAuditType<TResult>(type, scheme);
        if (grouped)
        {
            storage.PostponeRange(models, type.Value, additional);
        }
        else
        {
            foreach (var model in models)
            {
                storage.PostponeRange([model], type.Value, additional);
            }
        }
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
