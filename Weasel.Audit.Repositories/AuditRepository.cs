using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Services;

namespace Weasel.Audit.Repositories;

public interface IAuditRepository<T, TAudit>
    where T : class, IIntKeyedEntity, IAuditable<TAudit>
    where TAudit : class, IIntKeyedEntity
{
    DbContext DataBase { get; }
    IPostponedAuditManager AuditManager { get; }
    IAuditPropertyManager PropertyManager { get; }

    Task AddAsync(T model, int? userId, string? overrideLogin = null, Enum? overrideColor = null);
    Task AddCustomAsync(T model, int? userId, Enum customType, string? overrideLogin = null, Enum? overrideColor = null);
    Task AddRangeAsync(IReadOnlyList<T> models, int? userId, string? overrideLogin = null, Enum? overrideColor = null);
    Task AddCustomRangeAsync(IReadOnlyList<T> models, int? userId, Enum customType, string? overrideLogin = null, Enum? overrideColor = null);
    Task UpdateAsync(T oldModel, T newModel, int? userId, string? overrideLogin = null, Enum? overrideColor = null);
    Task UpdateCustomAsync(T oldModel, T newModel, int? userId, Enum customType, string? overrideLogin = null, Enum? overrideColor = null);
    Task UpdateRangeAsync(IReadOnlyList<Tuple<T, T>> models, int? userId, string? overrideLogin = null, Enum? overrideColor = null);
    Task UpdateCustomRangeAsync(IReadOnlyList<Tuple<T, T>> models, int? userId, Enum customType, string? overrideLogin = null, Enum? overrideColor = null);
    Task DeleteAsync(T model, int? userId, string? overrideLogin = null, Enum? overrideColor = null);
    Task DeleteCustomAsync(T model, int? userId, Enum customType, string? overrideLogin = null, Enum? overrideColor = null);
    Task DeleteRangeAsync(IReadOnlyList<T> models, int? userId, string? overrideLogin = null, Enum? overrideColor = null);
    Task DeleteCustomRangeAsync(IReadOnlyList<T> models, int? userId, Enum customType, string? overrideLogin = null, Enum? overrideColor = null);

    #region Events
    event BeforeInfoAuditDelegate<T>? BeforeAdd;
    event BeforeEditAuditDelegate<T>? BeforeUpdate;
    event BeforeInfoAuditDelegate<T>? BeforeDelete;
    #endregion

    #region Defaults LINQ
    IOrderedQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> filter);
    IOrderedQueryable<TAudit> OrderByAction<TKey>(Expression<Func<TAudit, TKey>> filter);
    IOrderedQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> filter);
    IOrderedQueryable<TAudit> OrderByDescendingAction<TKey>(Expression<Func<TAudit, TKey>> filter);
    Task<TKey> MaxAsync<TKey>(Expression<Func<T, TKey>> filter, CancellationToken token = default);
    IQueryable<T> Where(Expression<Func<T, bool>> filter);
    IQueryable<TAudit> WhereAction(Expression<Func<TAudit, bool>> filter);
    IQueryable<TProperty> Select<TProperty>(Expression<Func<T, TProperty>> filter);
    IQueryable<TProperty> SelectAction<TProperty>(Expression<Func<TAudit, TProperty>> filter);
    IIncludableQueryable<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> path);
    IIncludableQueryable<TAudit, TProperty> IncludeAction<TProperty>(Expression<Func<TAudit, TProperty>> path);
    Task AddActionAsync(TAudit value, CancellationToken token = default);
    Task AddRangeActionAsync(IEnumerable<TAudit> values, CancellationToken token = default);
    Task<TAudit?> FirstOrDefaultActionAsync(Expression<Func<TAudit, bool>> filter, CancellationToken token = default);
    void RemoveAction(TAudit value);
    Task AddAsync(T value, CancellationToken token = default);
    Task AddRangeAsync(IEnumerable<T> values, CancellationToken token = default);
    void Remove(T value);
    void RemoveRange(IEnumerable<T> value);
    IQueryable<T> GetSet();
    IQueryable<TAudit> GetActionSet();
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
public class AuditRepository<T, TAudit> : IAuditRepository<T, TAudit>
    where T : class, IIntKeyedEntity, IAuditable<TAudit>
    where TAudit : class, IIntKeyedEntity
{
    protected DbSet<T> Set { get; private set; }
    protected DbSet<TAudit> ActionSet { get; private set; }
    protected ILogger Logger { get; private set; }
    public DbContext DataBase { get; private set; }
    public IPostponedAuditManager AuditManager { get; private set; }
    public IAuditPropertyManager PropertyManager { get; private set; }

    public AuditRepository(DbContext context, ILoggerFactory loggerFactory, IPostponedAuditManager auditManager, IAuditPropertyManager propertyManager)
    {
        Logger = loggerFactory.CreateLogger(GetType().Name);
        DataBase = context;
        AuditManager = auditManager;
        PropertyManager = propertyManager;
        Set = DataBase.Set<T>();
        ActionSet = DataBase.Set<TAudit>();
    }

    public async Task AddAsync(T model, int? userId, string? overrideLogin = null, Enum? overrideColor = null)
    {
        await CallBeforeAdd(model);
        await AddAsync(model);
        AuditManager.PostponeCreate<T, TAudit>(model, userId, null, overrideLogin, overrideColor);
    }
    public async Task AddCustomAsync(T model, int? userId, Enum customType, string? overrideLogin = null, Enum? overrideColor = null)
    {
        await CallBeforeAdd(model);
        await AddAsync(model);
        AuditManager.PostponeCreate<T, TAudit>(model, userId, customType, overrideLogin, overrideColor);
    }
    public async Task AddRangeAsync(IReadOnlyList<T> models, int? userId, string? overrideLogin = null, Enum? overrideColor = null)
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
        AuditManager.PostponeCreateRange<T, TAudit>(models, userId, null, overrideLogin, overrideColor);
    }
    public async Task AddCustomRangeAsync(IReadOnlyList<T> models, int? userId, Enum customType, string? overrideLogin = null, Enum? overrideColor = null)
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
        AuditManager.PostponeCreateRange<T, TAudit>(models, userId, customType, overrideLogin, overrideColor);
    }

    private void Update(T oldModel, T newModel)
    {
        if (oldModel is ICustomUpdatable<T> oldCustomUpdatable)
        {
            oldCustomUpdatable.Update(newModel, DataBase);
        }
        else
        {
            PropertyManager.PerformAutoUpdate(DataBase, oldModel, newModel);
        }
    }

    public async Task UpdateAsync(T oldModel, T newModel, int? userId, string? overrideLogin = null, Enum? overrideColor = null)
    {
        await CallBeforeUpdate(oldModel, newModel);
        Update(oldModel, newModel);
        if (DataBase.Entry(oldModel).State != EntityState.Modified)
        {
            return;
        }
        Update(oldModel);
        AuditManager.PostponeUpdate<T, TAudit>(oldModel, userId, null, overrideLogin, overrideColor);
    }
    public async Task UpdateCustomAsync(T oldModel, T newModel, int? userId, Enum customType, string? overrideLogin = null, Enum? overrideColor = null)
    {
        await CallBeforeUpdate(oldModel, newModel);
        Update(oldModel, newModel);
        if (DataBase.Entry(oldModel).State != EntityState.Modified)
        {
            return;
        }
        Update(oldModel);
        AuditManager.PostponeUpdate<T, TAudit>(oldModel, userId, customType, overrideLogin, overrideColor);
    }
    public async Task UpdateRangeAsync(IReadOnlyList<Tuple<T, T>> models, int? userId, string? overrideLogin = null, Enum? overrideColor = null)
    {
        if (models.Count == 0)
        {
            return;
        }
        List<T> updateList = new List<T>();
        foreach (Tuple<T, T> model in models)
        {
            T oldModel = model.Item1;
            T newModel = model.Item2;
            await CallBeforeUpdate(oldModel, newModel);
            Update(oldModel, newModel);
            if (DataBase.Entry(oldModel).State != EntityState.Modified)
            {
                continue;
            }
            updateList.Add(oldModel);
        }
        if (updateList.Count == 0)
        {
            return;
        }
        UpdateRange(updateList);
        AuditManager.PostponeUpdateRange<T, TAudit>(updateList, userId, null, overrideLogin, overrideColor);
    }
    public async Task UpdateCustomRangeAsync(IReadOnlyList<Tuple<T, T>> models, int? userId, Enum customType, string? overrideLogin = null, Enum? overrideColor = null)
    {
        if (models.Count == 0)
        {
            return;
        }
        List<T> updateList = new List<T>();
        foreach (Tuple<T, T> model in models)
        {
            T oldModel = model.Item1;
            T newModel = model.Item2;
            await CallBeforeUpdate(oldModel, newModel);
            Update(oldModel, newModel);
            if (DataBase.Entry(oldModel).State != EntityState.Modified)
            {
                continue;
            }
            updateList.Add(oldModel);
        }
        if (updateList.Count == 0)
        {
            return;
        }
        UpdateRange(updateList);
        AuditManager.PostponeUpdateRange<T, TAudit>(updateList, userId, customType, overrideLogin, overrideColor);
    }

    public async Task DeleteAsync(T model, int? userId, string? overrideLogin = null, Enum? overrideColor = null)
    {
        await CallBeforeDelete(model);
        Remove(model);
        AuditManager.PostponeDelete<T, TAudit>(model, userId, null, overrideLogin, overrideColor);
    }
    public async Task DeleteCustomAsync(T model, int? userId, Enum customType, string? overrideLogin = null, Enum? overrideColor = null)
    {
        await CallBeforeDelete(model);
        Remove(model);
        AuditManager.PostponeDelete<T, TAudit>(model, userId, customType, overrideLogin, overrideColor);
    }
    public async Task DeleteRangeAsync(IReadOnlyList<T> models, int? userId, string? overrideLogin = null, Enum? overrideColor = null)
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
        AuditManager.PostponeDeleteRange<T, TAudit>(models, userId, null, overrideLogin, overrideColor);
    }
    public async Task DeleteCustomRangeAsync(IReadOnlyList<T> models, int? userId, Enum customType, string? overrideLogin = null, Enum? overrideColor = null)
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
        AuditManager.PostponeDeleteRange<T, TAudit>(models, userId, customType, overrideLogin, overrideColor);
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
    public IOrderedQueryable<TAudit> OrderByAction<TKey>(Expression<Func<TAudit, TKey>> filter)
        => ActionSet.OrderBy(filter);
    public IOrderedQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> filter)
        => Set.OrderByDescending(filter);
    public IOrderedQueryable<TAudit> OrderByDescendingAction<TKey>(Expression<Func<TAudit, TKey>> filter)
        => ActionSet.OrderByDescending(filter);
    public IQueryable<T> Where(Expression<Func<T, bool>> filter)
        => Set.Where(filter);
    public IQueryable<TAudit> WhereAction(Expression<Func<TAudit, bool>> filter)
        => ActionSet.Where(filter);
    public IQueryable<TProperty> Select<TProperty>(Expression<Func<T, TProperty>> filter)
        => Set.Select(filter);
    public IQueryable<TProperty> SelectAction<TProperty>(Expression<Func<TAudit, TProperty>> filter)
        => ActionSet.Select(filter);
    public IIncludableQueryable<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> path)
        => Set.Include(path);
    public IIncludableQueryable<TAudit, TProperty> IncludeAction<TProperty>(Expression<Func<TAudit, TProperty>> path)
        => ActionSet.Include(path);
    public async Task AddActionAsync(TAudit value, CancellationToken token = default)
        => await ActionSet.AddAsync(value, token);
    public async Task AddRangeActionAsync(IEnumerable<TAudit> values, CancellationToken token = default)
        => await ActionSet.AddRangeAsync(values, token);
    public async Task<TAudit?> FirstOrDefaultActionAsync(Expression<Func<TAudit, bool>> filter, CancellationToken token = default)
        => await ActionSet.FirstOrDefaultAsync(filter, token);
    public void RemoveAction(TAudit value)
        => ActionSet.Remove(value);

    public IQueryable<T> GetSet() => Set;
    public IQueryable<TAudit> GetActionSet() => ActionSet;

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
        => await DataBase.SaveChangesAsync(token);
    #endregion
}

public delegate Task BeforeInfoAuditDelegate<T>(T data);
public delegate Task BeforeEditAuditDelegate<T>(T old, T update);