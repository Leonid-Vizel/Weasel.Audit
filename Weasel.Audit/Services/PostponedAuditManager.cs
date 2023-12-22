using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
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
public interface IPostponedAuditManager<TAuditAction, TEnum, TColor>
    where TAuditAction : class, IAuditAction<TEnum>
    where TEnum : struct, Enum
    where TColor : struct, Enum
{
    IServiceProvider ServiceProvider { get; }
    IAuditActionFactory<TAuditAction, TEnum> ActionFactory { get; }
    IAuditSchemeManager<TEnum, TColor> SchemeManager { get; }
    PostponedAuditStorage<T, TAuditResult, TAuditAction, TEnum, TColor> GetOrAddStorage<T, TAuditResult>()
        where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
        where TAuditResult : class, IAuditResult<TAuditAction, TEnum>;
    void PostponeCreate<T, TAuditResult>(T model, TEnum? type = null, object? additional = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
        where TAuditResult : class, IAuditResult<TAuditAction, TEnum>;
    void PostponeCreateRange<T, TAuditResult>(IEnumerable<T> models, TEnum? type = null, object? additional = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
        where TAuditResult : class, IAuditResult<TAuditAction, TEnum>;
    void PostponeUpdate<T, TAuditResult>(T model, TEnum? type = null, object? additional = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
        where TAuditResult : class, IAuditResult<TAuditAction, TEnum>;
    void PostponeUpdateRange<T, TAuditResult>(IEnumerable<T> models, TEnum? type = null, object? additional = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
        where TAuditResult : class, IAuditResult<TAuditAction, TEnum>;
    void PostponeDelete<T, TAuditResult>(T model, TEnum? type = null, object? additional = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
        where TAuditResult : class, IAuditResult<TAuditAction, TEnum>;
    void PostponeDeleteRange<T, TAuditResult>(IEnumerable<T> models, TEnum? type = null, object? additional = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
        where TAuditResult : class, IAuditResult<TAuditAction, TEnum>;
    Task ExecuteAndDispose();
}
public sealed class PostponedAuditManager<TContext, TAuditAction, TEnum, TColor> : IPostponedAuditManager<TAuditAction, TEnum, TColor>
    where TAuditAction : class, IAuditAction<TEnum>
    where TContext : DbContext
    where TEnum : struct, Enum
    where TColor : struct, Enum
{
    private readonly Dictionary<PostponedAuditStorageKey, IPosponedActionsStorage<TAuditAction, TEnum, TColor>> _storages;
    private readonly ILogger<PostponedAuditManager<TContext, TAuditAction, TEnum, TColor>> _logger;
    public IServiceProvider ServiceProvider { get; private set; }
    public IAuditActionFactory<TAuditAction, TEnum> ActionFactory { get; private set; }
    public IAuditSchemeManager<TEnum, TColor> SchemeManager { get; private set; }
    public PostponedAuditManager(IServiceProvider provider, ILoggerFactory loggerFactory, IAuditSchemeManager<TEnum, TColor> schemeManager, IAuditActionFactory<TAuditAction, TEnum> actionFactory)
    {
        ServiceProvider = provider;
        SchemeManager = schemeManager;
        ActionFactory = actionFactory;
        _logger = loggerFactory.CreateLogger<PostponedAuditManager<TContext, TAuditAction, TEnum, TColor>>();
        _storages = [];
    }

    public PostponedAuditStorage<T, TAuditResult, TAuditAction, TEnum, TColor> GetOrAddStorage<T, TAuditResult>()
        where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
        where TAuditResult : class, IAuditResult<TAuditAction, TEnum>
    {
        var key = new PostponedAuditStorageKey(typeof(T), typeof(TAuditResult));
        if (!_storages.TryGetValue(key, out var storage))
        {
            storage = new PostponedAuditStorage<T, TAuditResult, TAuditAction, TEnum, TColor>(this);
            _storages.Add(key, storage);
        }
        return (PostponedAuditStorage<T, TAuditResult, TAuditAction, TEnum, TColor>)storage;
    }

    #region Create
    public void PostponeCreate<T, TAuditResult>(T model, TEnum? type, object? additional = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
        where TAuditResult : class, IAuditResult<TAuditAction, TEnum>
    {
        var storage = GetOrAddStorage<T, TAuditResult>();
        var scheme = custom ? AuditScheme.CustomCreate : AuditScheme.Create;
        type ??= SchemeManager.GetFirstSchemaAuditType<TAuditResult>(type, scheme);
        storage.Postpone(model, type.Value, additional);
    }
    public void PostponeCreateRange<T, TAuditResult>(IEnumerable<T> models, TEnum? type, object? additional = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
        where TAuditResult : class, IAuditResult<TAuditAction, TEnum>
    {
        var storage = GetOrAddStorage<T, TAuditResult>();
        var scheme = custom ? AuditScheme.CustomCreate : AuditScheme.Create;
        type ??= SchemeManager.GetFirstSchemaAuditType<TAuditResult>(type, scheme);
        storage.PostponeRange(models, type.Value, additional);
    }
    #endregion

    #region Update
    public void PostponeUpdate<T, TAuditResult>(T model, TEnum? type = null, object? additional = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
        where TAuditResult : class, IAuditResult<TAuditAction, TEnum>
    {
        var storage = GetOrAddStorage<T, TAuditResult>();
        var scheme = custom ? AuditScheme.CustomUpdate : AuditScheme.Update;
        type ??= SchemeManager.GetFirstSchemaAuditType<TAuditResult>(type, scheme);
        storage.Postpone(model, type.Value, additional);
    }
    public void PostponeUpdateRange<T, TAuditResult>(IEnumerable<T> models, TEnum? type = null, object? additional = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
        where TAuditResult : class, IAuditResult<TAuditAction, TEnum>
    {
        var storage = GetOrAddStorage<T, TAuditResult>();
        var scheme = custom ? AuditScheme.CustomUpdate : AuditScheme.Update;
        type ??= SchemeManager.GetFirstSchemaAuditType<TAuditResult>(type, scheme);
        storage.PostponeRange(models, type.Value, additional);
    }
    #endregion

    #region Delete
    public void PostponeDelete<T, TAuditResult>(T model, TEnum? type = null, object? additional = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
        where TAuditResult : class, IAuditResult<TAuditAction, TEnum>
    {
        var storage = GetOrAddStorage<T, TAuditResult>();
        var scheme = custom ? AuditScheme.CustomDelete : AuditScheme.Delete;
        type ??= SchemeManager.GetFirstSchemaAuditType<TAuditResult>(type, scheme);
        storage.Postpone(model, type.Value, additional);
    }
    public void PostponeDeleteRange<T, TAuditResult>(IEnumerable<T> models, TEnum? type = null, object? additional = null, bool custom = false)
        where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
        where TAuditResult : class, IAuditResult<TAuditAction, TEnum>
    {
        var storage = GetOrAddStorage<T, TAuditResult>();
        var scheme = custom ? AuditScheme.CustomDelete : AuditScheme.Delete;
        type ??= SchemeManager.GetFirstSchemaAuditType<TAuditResult>(type, scheme);
        storage.PostponeRange(models, type.Value, additional);
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
