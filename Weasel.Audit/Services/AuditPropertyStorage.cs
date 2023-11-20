using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Reflection;
using Weasel.Audit.Attributes.AuditUpdate;
using Weasel.Audit.Attributes.Display;
using Weasel.Audit.Enums;
using Weasel.Tools.Extensions.Common;

namespace Weasel.Audit.Services;

public struct AuditPropertyCacheKey
{
    public Type Type { get; private set; }
    public string PropertyName { get; private set; }
    public AuditPropertyCacheKey(PropertyInfo info)
    {
        if (info.DeclaringType == null)
        {
            throw new Exception($"DeclaringType of PropertyInfo is NULL. VB.NET modules are not supported by this library!");
        }
        Type = info.DeclaringType;
        PropertyName = info.Name;
    }
}
public struct AuditPropertyCache
{
    public string Name { get; private set; } = null!;
    public PropertyInfo Info { get; private set; } = null!;
    public Func<object, object> Getter { get; private set; } = null!;
    public Action<object, object> Setter { get; private set; } = null!;
    public AuditUpdateStrategyAttribute UpdateStrategy { get; private set; }
    public AuditDisplayStrategyAttribute DisplayStrategy { get; private set; }
    public AuditPropertyCache(AuditPropertyManager manager, PropertyInfo info)
    {
        Info = info;
        Getter = manager.CreatePropertyGetter(info);
        Setter = manager.CreatePropertySetter(info);
        Name = info.GetDisplayName() ?? info.Name;
        UpdateStrategy = info.GetCustomAttribute<AuditUpdateStrategyAttribute>() ??
            info.DeclaringType?.GetCustomAttribute<AuditUpdateStrategyAttribute>() ??
            new StandartAuditUpdateAttribute();
        DisplayStrategy = info.GetCustomAttribute<AuditDisplayStrategyAttribute>() ??
            info.DeclaringType?.GetCustomAttribute<AuditDisplayStrategyAttribute>() ??
            new StandartAuditDisplayAttribute();
    }
    public AuditPropertyDisplayMode GetDisplayMode(object? declare, object? value)
        => DisplayStrategy.GetDisplayMode(Info, declare, value);
    public string GetRowName(int index, object? declare, object? value)
        => DisplayStrategy.GetRowName(index, Info, declare, value);
    public object? FormatValue(object? declare, object? value)
        => DisplayStrategy.GetDisplayMode(Info, declare, value);
    public Type? GetCollectionType(object? declare, object? value)
        => DisplayStrategy.GetCollectionType(Info, declare, value);
    public bool Compare(DbContext context, object? old, object? update, object? oldValue, object? updateValue)
        => UpdateStrategy.Compare(context, old, update, oldValue, updateValue);
    public object? SetValue(DbContext context, object? old, object? update, object? oldValue, object? updateValue)
        => UpdateStrategy.SetValue(context, old, update, oldValue, updateValue);
}

public interface IAuditPropertyStorage
{
    ConcurrentDictionary<AuditPropertyCacheKey, AuditPropertyCache> CachedProperties { get; }
    List<AuditPropertyCache> GetAuditPropertyData<TAudit>(AuditPropertyManager manager);
    List<AuditPropertyCache> GetAuditPropertyData(AuditPropertyManager manager, Type type);
}
public sealed class AuditPropertyStorage : IAuditPropertyStorage
{
    public ConcurrentDictionary<AuditPropertyCacheKey, AuditPropertyCache> CachedProperties { get; private set; }
    public AuditPropertyStorage()
    {
        CachedProperties = new ConcurrentDictionary<AuditPropertyCacheKey, AuditPropertyCache>();
    }
    public List<AuditPropertyCache> GetAuditPropertyData<TAudit>(AuditPropertyManager manager)
        => GetAuditPropertyData(manager, typeof(TAudit));

    public List<AuditPropertyCache> GetAuditPropertyData(AuditPropertyManager manager, Type type)
    {
        var properties = type.GetProperties();
        var data = new List<AuditPropertyCache>();
        foreach (var info in properties)
        {
            var key = new AuditPropertyCacheKey(info);
            var createFunc = (AuditPropertyCacheKey key) => new AuditPropertyCache(manager, info);
            var cache = CachedProperties.GetOrAdd(key, createFunc);
            data.Add(cache);
        }
        return data;
    }
}
