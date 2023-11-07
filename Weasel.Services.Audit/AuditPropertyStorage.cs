using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
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
    public Func<object, object> Getter { get; private set; } = null!;
    public Action<object, object> Setter { get; private set; } = null!;
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
        else if (AuditPropertyManager.FieldTypes.Contains(Type))
        {
            DisplayMode = AuditPropertyDisplayMode.Field;
        }
        else if (Type.IsAssignableTo(typeof(ICollection)))
        {
            DisplayMode = AuditPropertyDisplayMode.List;
            RowNaming = info.GetCustomAttribute<AuditRowNamingRuleAttribute>();
        }
        Name = info.GetDisplayName() ?? info.Name;
    }
}

public interface IAuditPropertyStorage
{
    ConcurrentDictionary<AuditPropertyCacheKey, AuditPropertyCache> CachedProperties { get; }
}
public sealed class AuditPropertyStorage : IAuditPropertyStorage
{
    public ConcurrentDictionary<AuditPropertyCacheKey, AuditPropertyCache> CachedProperties { get; private set; }
    public AuditPropertyStorage()
    {
        CachedProperties = new ConcurrentDictionary<AuditPropertyCacheKey, AuditPropertyCache>();
    }

    public void GetAuditPropertyData<TAudit>() where TAudit : class
    {
        Type type = typeof(TAudit);
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var ignore = property.GetCustomAttribute()
        }
    }
}
