using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Services;

namespace Weasel.Audit.Repositories;

public interface IAuditRepository<T, TAuditResult, TAuditAction, TEnum>
    where TAuditResult : class, IAuditResult<TAuditAction, TEnum>
    where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
    where TAuditAction : class, IAuditAction<TEnum>
    where TEnum : struct, Enum
{
    DbContext Context { get; }
    IPostponedAuditManager<TAuditAction, TEnum> AuditManager { get; }
    IAuditPropertyManager PropertyManager { get; }
    DbSet<T> Set { get; }
    DbSet<TAuditResult> ActionSet { get; }
    ILogger Logger { get; }

    Task AuditAddAsync(T model, object? additional = null);
    Task AuditAddCustomAsync(T model, TEnum customType, object? additional = null);
    Task AuditAddRangeAsync(IReadOnlyList<T> models, object? additional = null);
    Task AuditAddCustomRangeAsync(IReadOnlyList<T> models, TEnum customType, object? additional = null);
    Task AuditUpdateAsync(T oldModel, T updateModel, object? additional = null);
    Task AuditUpdateCustomAsync(T oldModel, T updateModel, TEnum customType, object? additional = null);
    Task AuditUpdateRangeAsync(IReadOnlyList<Tuple<T, T>> models, object? additional = null);
    Task AuditUpdateCustomRangeAsync(IReadOnlyList<Tuple<T, T>> models, TEnum customType, object? additional = null);
    Task AuditDeleteAsync(T model, object? additional = null);
    Task AuditDeleteCustomAsync(T model, TEnum customType, object? additional = null);
    Task AuditDeleteRangeAsync(IReadOnlyList<T> models, object? additional = null);
    Task AuditDeleteCustomRangeAsync(IReadOnlyList<T> models, TEnum customType, object? additional = null);

    #region Events
    event BeforeInfoAuditDelegate<T>? BeforeAdd;
    event BeforeEditAuditDelegate<T>? BeforeUpdate;
    event BeforeInfoAuditDelegate<T>? BeforeDelete;
    #endregion

    #region Defaults LINQ
    IOrderedQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> filter);
    IOrderedQueryable<TAuditResult> OrderByAction<TKey>(Expression<Func<TAuditResult, TKey>> filter);
    IOrderedQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> filter);
    IOrderedQueryable<TAuditResult> OrderByDescendingAction<TKey>(Expression<Func<TAuditResult, TKey>> filter);
    Task<TKey> MaxAsync<TKey>(Expression<Func<T, TKey>> filter, CancellationToken token = default);
    IQueryable<T> Where(Expression<Func<T, bool>> filter);
    IQueryable<TAuditResult> WhereAction(Expression<Func<TAuditResult, bool>> filter);
    IQueryable<TProperty> Select<TProperty>(Expression<Func<T, TProperty>> filter);
    IQueryable<TProperty> SelectAction<TProperty>(Expression<Func<TAuditResult, TProperty>> filter);
    IIncludableQueryable<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> path);
    IIncludableQueryable<TAuditResult, TProperty> IncludeAction<TProperty>(Expression<Func<TAuditResult, TProperty>> path);
    Task AddActionAsync(TAuditResult value, CancellationToken token = default);
    Task AddRangeActionAsync(IEnumerable<TAuditResult> values, CancellationToken token = default);
    Task<TAuditResult?> FirstOrDefaultActionAsync(Expression<Func<TAuditResult, bool>> filter, CancellationToken token = default);
    void RemoveAction(TAuditResult value);
    Task AddAsync(T value, CancellationToken token = default);
    Task AddRangeAsync(IEnumerable<T> values, CancellationToken token = default);
    void Remove(T value);
    void RemoveRange(IEnumerable<T> value);
    IQueryable<T> GetSet();
    IQueryable<TAuditResult> GetActionSet();
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, CancellationToken token = default);
    Task<int> CountAsync(CancellationToken token = default);
    Task<int> CountAsync(Expression<Func<T, bool>> filter, CancellationToken token = default);
    Task<bool> AnyAsync(CancellationToken token = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> filter, CancellationToken token = default);
    Task<bool> AllAsync(Expression<Func<T, bool>> filter, CancellationToken token = default);
    void Update(T value);
    void UpdateRange(IEnumerable<T> value);
    #endregion
}
public class AuditRepository<T, TAuditResult, TAuditAction, TEnum> : IAuditRepository<T, TAuditResult, TAuditAction, TEnum>
    where T : class, IAuditable<TAuditResult, TAuditAction, TEnum>
    where TAuditResult : class, IAuditResult<TAuditAction, TEnum>
    where TAuditAction: class, IAuditAction<TEnum>
	where TEnum : struct, Enum
{
    public DbSet<T> Set { get; private set; }
    public DbSet<TAuditResult> ActionSet { get; private set; }
    public ILogger Logger { get; private set; }
    public DbContext Context { get; private set; }
    public IPostponedAuditManager<TAuditAction, TEnum> AuditManager { get; private set; }
    public IAuditPropertyManager PropertyManager { get; private set; }

    public AuditRepository(DbContext context, ILoggerFactory loggerFactory, IPostponedAuditManager<TAuditAction, TEnum> auditManager, IAuditPropertyManager propertyManager)
    {
        Logger = loggerFactory.CreateLogger(GetType().Name);
        Context = context;
        AuditManager = auditManager;
        PropertyManager = propertyManager;
        Set = Context.Set<T>();
        ActionSet = Context.Set<TAuditResult>();
    }

    public async Task AuditAddAsync(T model, object? additional = null)
        => await PerformAddAsync(model, null, additional);
    public async Task AuditAddCustomAsync(T model, TEnum customType, object? additional = null)
        => await PerformAddAsync(model, customType, additional);
    public async Task AuditAddRangeAsync(IReadOnlyList<T> models, object? additional = null)
        => await PerformAddRangeAsync(models, null, additional);
    public async Task AuditAddCustomRangeAsync(IReadOnlyList<T> models, TEnum customType, object? additional = null)
        => await PerformAddRangeAsync(models, customType, additional);
    public async Task AuditUpdateAsync(T oldModel, T updateModel, object? additional = null)
        => await PerformUpdateAsync(oldModel, updateModel, null, additional);
    public async Task AuditUpdateCustomAsync(T oldModel, T updateModel, TEnum customType, object? additional = null)
        => await PerformUpdateAsync(oldModel, updateModel, customType, additional);
    public async Task AuditUpdateRangeAsync(IReadOnlyList<Tuple<T, T>> models, object? additional = null)
        => await PerformUpdateRangeAsync(models, null, additional);
    public async Task AuditUpdateCustomRangeAsync(IReadOnlyList<Tuple<T, T>> models, TEnum customType, object? additional = null)
        => await PerformUpdateRangeAsync(models, customType, additional);
    public async Task AuditDeleteAsync(T model, object? additional = null)
        => await PerformDeleteAsync(model, null, additional);
    public async Task AuditDeleteCustomAsync(T model, TEnum customType, object? additional = null)
        => await PerformDeleteAsync(model, customType, additional);
    public async Task AuditDeleteRangeAsync(IReadOnlyList<T> models, object? additional = null)
        => await PerformDeleteRangeAsync(models, null, additional);
    public async Task AuditDeleteCustomRangeAsync(IReadOnlyList<T> models, TEnum customType, object? additional = null)
        => await PerformDeleteRangeAsync(models, customType, additional);

    private async Task PerformAddAsync(T model, TEnum? auditType = null, object? additional = null)
    {
        await CallBeforeAdd(model);
        await AddAsync(model);
        AuditManager.PostponeUpdate<T, TAuditResult>(model, auditType, additional, auditType != null);
    }
    private async Task PerformAddRangeAsync(IReadOnlyList<T> models, TEnum? auditType = null, object? additional = null)
    {
        if (models.Count == 0)
        {
            return;
        }
        foreach (T model in models)
        {
            await CallBeforeAdd(model);
        }
        await AddRangeAsync(models);
        AuditManager.PostponeCreateRange<T, TAuditResult>(models, auditType, additional, auditType != null);
    }
    private async Task PerformUpdateAsync(T oldModel, T updateModel, TEnum? auditType = null, object? additional = null)
    {
        await CallBeforeUpdate(oldModel, updateModel);
        PropertyManager.PerformUpdate(Context, oldModel, updateModel);
        if (Context.Entry(oldModel).State != EntityState.Modified)
        {
            return;
        }
        Update(oldModel);
        AuditManager.PostponeUpdate<T, TAuditResult>(oldModel, auditType, additional, auditType != null);
    }
    private async Task PerformUpdateRangeAsync(IReadOnlyList<Tuple<T, T>> models, TEnum? auditType = null, object? additional = null)
    {
        if (models.Count == 0)
        {
            return;
        }
        foreach (Tuple<T, T> model in models)
        {
            await CallBeforeUpdate(model.Item1, model.Item2);
        }
        PropertyManager.PerformUpdateRange(Context, models);
        List<T> updateList = new List<T>();
        foreach (Tuple<T, T> model in models)
        {
            var entry = Context.Entry(model.Item1);
            entry.DetectChanges();
            if (entry.State == EntityState.Modified)
            {
                updateList.Add(model.Item1);
            }
        }
        if (updateList.Count == 0)
        {
            return;
        }
        UpdateRange(updateList);
        AuditManager.PostponeUpdateRange<T, TAuditResult>(updateList, auditType, additional, auditType != null);
    }
    private async Task PerformDeleteAsync(T model, TEnum? auditType = null, object? additional = null)
    {
        await CallBeforeDelete(model);
        Remove(model);
        AuditManager.PostponeDelete<T, TAuditResult>(model, auditType, additional, auditType != null);
    }
    private async Task PerformDeleteRangeAsync(IReadOnlyList<T> models, TEnum? auditType = null, object? additional = null)
    {
        if (models.Count == 0)
        {
            return;
        }
        for (int i = 0; i < models.Count; i++)
        {
            T model = models[i];
            await CallBeforeDelete(model);
        }
        RemoveRange(models);
        AuditManager.PostponeDeleteRange<T, TAuditResult>(models, auditType, additional, auditType != null);
    }

    #region Events
    private async Task CallBeforeAdd(T data)
    {
        if (BeforeAdd != null)
        {
            await BeforeAdd.Invoke(data);
        }
    }
    private async Task CallBeforeUpdate(T old, T update)
    {
        if (BeforeUpdate != null)
        {
            await BeforeUpdate.Invoke(old, update);
        }
    }
    private async Task CallBeforeDelete(T data)
    {
        if (BeforeDelete != null)
        {
            await BeforeDelete.Invoke(data);
        }
    }

    public event BeforeInfoAuditDelegate<T>? BeforeAdd;
    public event BeforeEditAuditDelegate<T>? BeforeUpdate;
    public event BeforeInfoAuditDelegate<T>? BeforeDelete;
    #endregion

    #region Defaults (LINQ)
    public IOrderedQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> filter)
        => Set.OrderBy(filter);
    public IOrderedQueryable<TAuditResult> OrderByAction<TKey>(Expression<Func<TAuditResult, TKey>> filter)
        => ActionSet.OrderBy(filter);
    public IOrderedQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> filter)
        => Set.OrderByDescending(filter);
    public IOrderedQueryable<TAuditResult> OrderByDescendingAction<TKey>(Expression<Func<TAuditResult, TKey>> filter)
        => ActionSet.OrderByDescending(filter);
    public IQueryable<T> Where(Expression<Func<T, bool>> filter)
        => Set.Where(filter);
    public IQueryable<TAuditResult> WhereAction(Expression<Func<TAuditResult, bool>> filter)
        => ActionSet.Where(filter);
    public IQueryable<TProperty> Select<TProperty>(Expression<Func<T, TProperty>> filter)
        => Set.Select(filter);
    public IQueryable<TProperty> SelectAction<TProperty>(Expression<Func<TAuditResult, TProperty>> filter)
        => ActionSet.Select(filter);
    public IIncludableQueryable<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> path)
        => Set.Include(path);
    public IIncludableQueryable<TAuditResult, TProperty> IncludeAction<TProperty>(Expression<Func<TAuditResult, TProperty>> path)
        => ActionSet.Include(path);
    public async Task AddActionAsync(TAuditResult value, CancellationToken token = default)
        => await ActionSet.AddAsync(value, token);
    public async Task AddRangeActionAsync(IEnumerable<TAuditResult> values, CancellationToken token = default)
        => await ActionSet.AddRangeAsync(values, token);
    public async Task<TAuditResult?> FirstOrDefaultActionAsync(Expression<Func<TAuditResult, bool>> filter, CancellationToken token = default)
        => await ActionSet.FirstOrDefaultAsync(filter, token);
    public void RemoveAction(TAuditResult value)
        => ActionSet.Remove(value);

    public IQueryable<T> GetSet()
        => Set;
    public IQueryable<TAuditResult> GetActionSet()
        => ActionSet;

    public async Task AddAsync(T value, CancellationToken token = default)
        => await Set.AddAsync(value, token);
    public async Task AddRangeAsync(IEnumerable<T> values, CancellationToken token = default)
        => await Set.AddRangeAsync(values, token);
    public async Task<int> CountAsync(CancellationToken token = default)
        => await Set.CountAsync();
    public async Task<int> CountAsync(Expression<Func<T, bool>> filter, CancellationToken token = default)
        => await Set.CountAsync(filter, token);
    public async Task<TKey> MaxAsync<TKey>(Expression<Func<T, TKey>> filter, CancellationToken token = default)
        => await Set.MaxAsync(filter, token);
    public async Task<bool> AnyAsync(CancellationToken token = default)
        => await Set.AnyAsync(token);
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter, CancellationToken token = default)
        => await Set.AnyAsync(filter, token);
    public async Task<bool> AllAsync(Expression<Func<T, bool>> filter, CancellationToken token = default)
        => await Set.AllAsync(filter, token);
    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, CancellationToken token = default)
        => await Set.FirstOrDefaultAsync(filter, token);
    public void Remove(T value)
        => Set.Remove(value);
    public void Update(T value)
        => Set.Update(value);
    public void RemoveRange(IEnumerable<T> value)
        => Set.RemoveRange(value);
    public void UpdateRange(IEnumerable<T> value)
        => Set.UpdateRange(value);
    public async Task SaveAsync(CancellationToken token = default)
        => await Context.SaveChangesAsync(token);
    #endregion
}

public delegate Task BeforeInfoAuditDelegate<T>(T data);
public delegate Task BeforeEditAuditDelegate<T>(T old, T update);