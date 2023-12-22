using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using Weasel.Audit.Attributes.Enums;
using Weasel.Audit.Enums;

namespace Weasel.Audit.Services;

public struct TypeAuditSchemeKey
{
    public Type Type { get; private set; }
    public AuditScheme Scheme { get; private set; }
    public TypeAuditSchemeKey(Type type, AuditScheme scheme)
    {
        Type = type;
        Scheme = scheme;
    }
}

public interface IAuditSchemeManager<TEnum, TColor>
    where TEnum : struct, Enum
    where TColor : struct, Enum
{
    AuditDescAttribute? GetAuditEnumDescription(TEnum type);
    AuditColorAttribute? GetAuditColorDescription(TColor color);
    Type? GetAuditEnumActionType(TEnum type);
    Type? GetTypeBySearchName(string name);
    TEnum[] GetSchemaAuditTypes<TAudit>(AuditScheme scheme);
    TEnum[] GetSchemaAuditTypes(AuditScheme scheme, Type actionType);
    TEnum GetFirstSchemaAuditType<TAudit>(TEnum? type, AuditScheme scheme);
    TEnum GetFirstSchemaAuditType(Type actionType, TEnum? type, AuditScheme scheme);
    List<TEnum> GetTypeActions<TAudit>();
    List<TEnum> GetTypeActions(Type actionType);
}

public sealed class AuditSchemeManager<TEnum, TColor> : IAuditSchemeManager<TEnum, TColor>
    where TEnum : struct, Enum
    where TColor : struct, Enum
{
    private readonly ConcurrentDictionary<Type, List<TEnum>> _typeActions;
    private readonly ConcurrentDictionary<TypeAuditSchemeKey, TEnum[]> _typeSchemaActions;
    private readonly FrozenDictionary<TEnum, AuditDescAttribute> _enumDescriptions;
    private readonly FrozenDictionary<Enum, AuditColorAttribute> _colorDescriptions;
    private readonly FrozenDictionary<string, Type> _auditTypeSearchDictionary;
    public AuditSchemeManager()
    {
        _typeActions = new ConcurrentDictionary<Type, List<TEnum>>();
        _typeSchemaActions = new ConcurrentDictionary<TypeAuditSchemeKey, TEnum[]>();
        var colorDescDict = new Dictionary<Enum, AuditColorAttribute>();
        var enumDescDict = new Dictionary<TEnum, AuditDescAttribute>();
        var auditTypeSearchDict = new Dictionary<string, Type>();
        foreach (TEnum type in Enum.GetValues<TEnum>())
        {
            var decription = GetAuditEnumDescription(type);
            if (decription == null)
            {
                throw new ArgumentNullException($"Provide {nameof(AuditDescAttribute)} attribute for {type}!");
            }
            enumDescDict.TryAdd(type, decription);
            auditTypeSearchDict.TryAdd(decription.SearchTypeName, decription.Type);
        }
        foreach (TColor color in Enum.GetValues<TColor>())
        {
            var description = typeof(TColor).GetMember(color.ToString()).FirstOrDefault()?.GetCustomAttribute<AuditColorAttribute>();
            if (description == null)
            {
                continue;
            }
            colorDescDict.TryAdd(color, description);
        }
        _enumDescriptions = enumDescDict.ToFrozenDictionary();
        _colorDescriptions = colorDescDict.ToFrozenDictionary();
        _auditTypeSearchDictionary = auditTypeSearchDict.ToFrozenDictionary();
    }

    #region GetTypeBySearchName
    public Type? GetTypeBySearchName(string name)
    {
        if (_auditTypeSearchDictionary.TryGetValue(name, out var type))
        {
            return type;
        }
        return null;
    }
    #endregion

    #region GetAuditEnumDescription
    public AuditDescAttribute? GetAuditEnumDescription(TEnum type)
        => _enumDescriptions[type];
    #endregion

    #region GetAuditColorDescription
    public AuditColorAttribute? GetAuditColorDescription(TColor color)
    {
        if (!_colorDescriptions.TryGetValue(color, out var value))
        {
            return null;
        }
        return value;
    }
    #endregion

    #region GetAuditEnumActionType
    public Type? GetAuditEnumActionType(TEnum type)
    {
        var description = GetAuditEnumDescription(type);
        if (description == null)
        {
            return null;
        }
        return description.Type;
    }
    #endregion

    #region GetSchemaAuditTypes
    public TEnum[] GetSchemaAuditTypes<TAudit>(AuditScheme scheme)
        => GetSchemaAuditTypes(scheme, typeof(TAudit));
    public TEnum[] GetSchemaAuditTypes(AuditScheme scheme, Type actionType)
    {
        var key = new TypeAuditSchemeKey(actionType, scheme);
        if (!_typeSchemaActions.TryGetValue(key, out var action))
        {
            action = _enumDescriptions.Where(x => x.Value.Scheme == scheme && x.Value.Type == actionType).Select(x => x.Key).ToArray();
            _typeSchemaActions.TryAdd(key, action);
        }
        return action;
    }
    #endregion

    #region GetFirstSchemaAuditType
    public TEnum GetFirstSchemaAuditType<TAudit>(TEnum? type, AuditScheme scheme)
        => GetFirstSchemaAuditType(typeof(TAudit), type, scheme);

    public TEnum GetFirstSchemaAuditType(Type actionType, TEnum? type, AuditScheme scheme)
    {
        if (type != null)
        {
            return type.Value;
        }
        var types = GetSchemaAuditTypes(scheme, actionType);
        if (types.Length <= 0)
        {
            throw new Exception($"Cant find action type enum for {scheme} and {actionType}!");
        }
        return types[0];
    }
    #endregion

    #region GetTypeActions
    public List<TEnum> GetTypeActions<TAudit>()
        => GetTypeActions(typeof(TAudit));
    public List<TEnum> GetTypeActions(Type actionType)
    {
        if (!_typeActions.TryGetValue(actionType, out var list))
        {
            list = _enumDescriptions.Where(x => x.Value.Type == actionType).Select(x => x.Key).ToList();
            _typeActions.TryAdd(actionType, list);
        }
        return list;
    }
    #endregion
}
