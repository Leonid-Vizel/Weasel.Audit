using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Concurrent;
using System.Reflection;
using Weasel.Attributes;
using Weasel.Audit.Interfaces;

namespace Weasel.Tools.Extensions.EFCore;

public static class DbContextExtensions
{
    private static ConcurrentDictionary<IncludeAllCacheKey, List<string>> _actionIncludePaths
        = new ConcurrentDictionary<IncludeAllCacheKey, List<string>>();
    private static readonly MethodInfo setMethod
        = typeof(DbContext).GetMethods().Single(p => p.Name == nameof(DbContext.Set) && p.ContainsGenericParameters && p.GetParameters().Length == 0);

    public static IEnumerable<string> GetIncludePaths<T>(this DbContext context, int depth = 20)
        => context.GetIncludePaths(typeof(T), depth);
    public static IEnumerable<string> GetIncludePaths(this DbContext context, Type type, int depth = 20)
    {
        var key = new IncludeAllCacheKey(type, depth);
        if (!_actionIncludePaths.TryGetValue(key, out var list))
        {
            list = CalculateIncludePaths(context, type, depth).ToList();
            _actionIncludePaths.TryAdd(key, list);
        }
        return list;
    }
    private static IEnumerable<string> CalculateIncludePaths(this DbContext context, Type clrEntityType, int depth = 20)
    {
        var entityType = context.Model.FindEntityType(clrEntityType);
        if (entityType == null)
        {
            yield break;
        }
        var stack = new Stack<IEnumerator<INavigation>>();
        bool flag = true;
        while (flag)
        {
            var entityNavigations = new List<INavigation>();
            if (stack.Count <= depth)
            {
                foreach (var navigation in entityType.GetNavigations())
                {
                    if (navigation.PropertyInfo?.GetCustomAttribute<PreventCycleAttribute>() == null)
                    {
                        entityNavigations.Add(navigation);
                    }
                }
            }
            if (entityNavigations.Count == 0)
            {
                if (stack.Count > 0)
                {
                    yield return string.Join(".", stack.Reverse().Select(e => e.Current.Name));
                }
            }
            else
            {
                stack.Push(entityNavigations.GetEnumerator());
            }
            while (stack.Count > 0 && !stack.Peek().MoveNext())
            {
                stack.Pop();
            }
            if (stack.Count == 0)
            {
                flag = false;
            }
            else
            {
                entityType = stack.Peek().Current.TargetEntityType;
            }
        }
    }
    public static IQueryable<IIntKeyedEntity>? GetQueryable(this DbContext context, Type type)
        => setMethod.MakeGenericMethod(type).Invoke(context, null) as IQueryable<IIntKeyedEntity>;
    public static IQueryable<IIntKeyedEntity> IncludeAll(this DbContext context, Type type, int depth = 20)
    {
        var query = context.GetQueryable(type);
        if (query == null)
        {
            throw new Exception($"Cant get set of {type.FullName} from passed DbContext instance");
        }
        var paths = context.GetIncludePaths(type, depth);
        foreach (var path in paths)
        {
            query = query.Include(path);
        }
        return query;
    }
}
