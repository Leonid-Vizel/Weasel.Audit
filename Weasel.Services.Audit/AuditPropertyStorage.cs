using ECRF.Tools.Actions.Interfaces;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Weasel.Attributes.Audit.AutoUpdate.Strategy;
using Weasel.Attributes.Audit.Display;
using Weasel.Attributes.Audit.Formatters;
using Weasel.Attributes.Audit.Rows;
using Weasel.Enums;
using Weasel.Tools.Extensions.Common;

namespace Weasel.Services.Audit;

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
    public Type Type { get; private set; } = null!;
    public Type? InnerListType { get; private set; } = null!;
    public Func<object, object> Getter { get; private set; } = null!;
    public Action<object, object> Setter { get; private set; } = null!;
    public AutoUpdateStrategyAttribute.CompareDelegate? CompareDelegate { get; private set; }
    public AutoUpdateStrategyAttribute.SetValueDelegate? SetValueDelegate { get; private set; }
    public AutoUpdateStrategyAttribute? AutoUpdateStrategy { get; private set; }
    public AuditValueFormatterAttribute? ValueFormatter { get; private set; }
    public AuditRowNamingRuleAttribute? RowNaming { get; private set; }
    public AuditPropertyDisplayMode DisplayMode { get; private set; }
    public bool AutoUpdate { get; private set; }
    public AuditPropertyCache(AuditPropertyManager manager, PropertyInfo info)
    {
        Type = info.PropertyType;
        Getter = manager.CreatePropertyGetter(info);
        Setter = manager.CreatePropertySetter(info);
        ValueFormatter = info.GetCustomAttribute<AuditValueFormatterAttribute>();
        if (info.GetCustomAttribute<AuditDisplayIgnoreAttribute>() != null)
        {
            DisplayMode = AuditPropertyDisplayMode.None;
        }
        else if (info.Name.EndsWith("Id") && info.GetCustomAttribute<AuditDisplayForceAttribute>() == null)
        {
            DisplayMode = AuditPropertyDisplayMode.None;
        }
        else if (Type.IsAssignableTo(typeof(Enum)))
        {
            DisplayMode = AuditPropertyDisplayMode.Enum;
        }
        else if (AuditPropertyManager.FieldTypes.Contains(Type))
        {
            DisplayMode = AuditPropertyDisplayMode.Field;
        }
        else if (Type.IsAssignableTo(typeof(ICollection)))
        {
            DisplayMode = AuditPropertyDisplayMode.List;
            InnerListType = info.PropertyType.GetGenericArguments()[0];
            RowNaming = info.GetCustomAttribute<AuditRowNamingRuleAttribute>();
        }
        Name = info.GetDisplayName() ?? info.Name;
        if (info.GetCustomAttribute<AuditDisplayIgnoreAttribute>() != null)
        {
            DisplayMode = AuditPropertyDisplayMode.None;
        }
        else if (info.Name.EndsWith("Id") && info.GetCustomAttribute<AuditDisplayForceAttribute>() == null)
        {
            DisplayMode = AuditPropertyDisplayMode.None;
        }
        AutoUpdate = info.GetCustomAttribute<AuditDisplayIgnoreAttribute>() == null ||
            (info.Name.EndsWith("Id") && info.GetCustomAttribute<AuditDisplayForceAttribute>() != null);
        if (AutoUpdate)
        {
            var customStrategy = info.GetCustomAttribute<AutoUpdateStrategyAttribute>();
            if (customStrategy == null)
            {
                CompareDelegate = StandartAutoUpdateStrategyAttribbute.CompareBoxedValues;
                SetValueDelegate = StandartAutoUpdateStrategyAttribbute.StandartSetValue;
            }
            else
            {
                CompareDelegate = customStrategy.Compare;
                SetValueDelegate = customStrategy.SetValue;
            }
        }
    }
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
        List<AuditPropertyCache> data = new List<AuditPropertyCache>();
        foreach (var info in properties)
        {
            var key = new AuditPropertyCacheKey(info);
            var createFunc = (AuditPropertyCacheKey key) => new AuditPropertyCache(manager, info);
            var cache = CachedProperties.GetOrAdd(key, createFunc);
        }
        return data;
    }
}
