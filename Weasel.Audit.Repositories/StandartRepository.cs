using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Weasel.Audit.Repositories;

public interface IStandartRepository<T> where T : class
{
    DbSet<T> Set { get; }
    DbContext Context { get; }
    DbSet<T> GetSet();
    Task AddAsync(T model);
    Task AddRangeAsync(IEnumerable<T> models);
    void Update(T model);
    void UpdateRange(IEnumerable<T> models);
    void Delete(T model);
    void DeleteRange(IEnumerable<T> models);
    Task<bool> AllAsync(Expression<Func<T, bool>> filter);
    Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
    Task<bool> AnyAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> filter);
    Task<long> LongCountAsync(Expression<Func<T, bool>> filter);
    IOrderedQueryable<T> OrderBy(Expression<Func<T, bool>> filter);
    IOrderedQueryable<T> OrderByDescending(Expression<Func<T, bool>> filter);
    IQueryable<T> Where(Expression<Func<T, bool>> filter);
    IQueryable<TProperty> Select<TProperty>(Expression<Func<T, TProperty>> filter);
    IIncludableQueryable<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> path);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter);
    Task<T?> LastOrDefaultAsync(Expression<Func<T, bool>> filter);
    Task SaveAsync();
}

public class StandartRepository<T> : IStandartRepository<T> where T : class
{
    public DbSet<T> Set { get; private set; } = null!;
    public DbContext Context { get; private set; } = null!;
    public StandartRepository(DbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }
    public DbSet<T> GetSet()
        => Set;
    public async Task AddAsync(T model)
        => await Set.AddAsync(model);
    public async Task AddRangeAsync(IEnumerable<T> models)
        => await Set.AddRangeAsync(models);
    public void Update(T model)
        => Set.Update(model);
    public void UpdateRange(IEnumerable<T> models)
        => Set.UpdateRange(models);
    public void Delete(T model)
        => Set.Remove(model);
    public void DeleteRange(IEnumerable<T> models)
        => Set.RemoveRange(models);
    public async Task<bool> AllAsync(Expression<Func<T, bool>> filter)
        => await Set.AllAsync(filter);
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter)
        => await Set.AnyAsync(filter);
    public async Task<bool> AnyAsync()
        => await Set.AnyAsync();
    public async Task<int> CountAsync(Expression<Func<T, bool>> filter)
        => await Set.CountAsync(filter);
    public async Task<long> LongCountAsync(Expression<Func<T, bool>> filter)
        => await Set.LongCountAsync(filter);
    public IOrderedQueryable<T> OrderBy(Expression<Func<T, bool>> filter)
        => Set.OrderBy(filter);
    public IOrderedQueryable<T> OrderByDescending(Expression<Func<T, bool>> filter)
        => Set.OrderByDescending(filter);
    public IQueryable<T> Where(Expression<Func<T, bool>> filter)
        => Set.Where(filter);
    public IQueryable<TProperty> Select<TProperty>(Expression<Func<T, TProperty>> filter)
        => Set.Select(filter);
    public IIncludableQueryable<T, TProperty> Include<TProperty>(Expression<Func<T, TProperty>> path)
        => Set.Include(path);
    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter)
        => await Set.FirstOrDefaultAsync(filter);
    public async Task<T?> LastOrDefaultAsync(Expression<Func<T, bool>> filter)
        => await Set.LastOrDefaultAsync(filter);
    public async Task SaveAsync()
        => await Context.SaveChangesAsync();
}
