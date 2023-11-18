using System.Collections.Concurrent;
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

public interface IAuditSchemeManager
{
    AuditDescAttribute? GetAuditEnumDescription(Enum type);
    Type? GetAuditEnumActionType(Enum type);
    Type? GetTypeBySearchName(string name);
    Enum[] GetSchemaAuditTypes<TAudit>(AuditScheme scheme);
    Enum[] GetSchemaAuditTypes(AuditScheme scheme, Type actionType);
    Enum GetFirstSchemaAuditType<TAudit>(Enum? type, AuditScheme scheme);
    Enum GetFirstSchemaAuditType(Type actionType, Enum? type, AuditScheme scheme);
    List<Enum> GetTypeActions<TAudit>();
    List<Enum> GetTypeActions(Type actionType);
}

public sealed class AuditSchemeManager : IAuditSchemeManager
{
    private readonly ConcurrentDictionary<Type, List<Enum>> _typeActions;
    private readonly ConcurrentDictionary<Enum, AuditDescAttribute> _enumDescriptions;
    private readonly ConcurrentDictionary<string, Type> _auditTypeSearchDictionary;
    private readonly ConcurrentDictionary<TypeAuditSchemeKey, Enum[]> _typeSchemaActions;
    public AuditSchemeManager(Type[] enumTypes)
    {
        _typeActions = new ConcurrentDictionary<Type, List<Enum>>();
        _typeSchemaActions = new ConcurrentDictionary<TypeAuditSchemeKey, Enum[]>();
        _enumDescriptions = new ConcurrentDictionary<Enum, AuditDescAttribute>();
        _auditTypeSearchDictionary = new ConcurrentDictionary<string, Type>();
        foreach (var enumType in enumTypes.Where(x => x.IsEnum))
        {
            foreach (Enum type in Enum.GetValues(enumType))
            {
                var decription = GetAuditEnumDescription(type);
                if (decription == null)
                {
                    throw new ArgumentNullException($"Provide {nameof(AuditDescAttribute)} attribute for {type}!");
                }
                _enumDescriptions.TryAdd(type, decription);
                _auditTypeSearchDictionary.TryAdd(decription.SearchTypeName, decription.Type);
            }
        }
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
    public AuditDescAttribute? GetAuditEnumDescription(Enum type)
    {
        if (_enumDescriptions.TryGetValue(type, out var value))
        {
            return value;
        }
        value = type.GetType()
            .GetMember(type.ToString())
            .FirstOrDefault()?
            .GetCustomAttribute<AuditDescAttribute>();
        if (value != null)
        {
            _enumDescriptions[type] = value;
        }
        return value;
    }
    #endregion

    #region GetAuditEnumActionType
    public Type? GetAuditEnumActionType(Enum type)
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
    public Enum[] GetSchemaAuditTypes<TAudit>(AuditScheme scheme)
        => GetSchemaAuditTypes(scheme, typeof(TAudit));
    public Enum[] GetSchemaAuditTypes(AuditScheme scheme, Type actionType)
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
    public Enum GetFirstSchemaAuditType<TAudit>(Enum? type, AuditScheme scheme)
        => GetFirstSchemaAuditType(typeof(TAudit), type, scheme);

    public Enum GetFirstSchemaAuditType(Type actionType, Enum? type, AuditScheme scheme)
    {
        if (type != null)
        {
            return type;
        }
        var types = GetSchemaAuditTypes(scheme, actionType);
        var foundType = types.FirstOrDefault();
        if (foundType == null)
        {
            throw new Exception($"Cant find action type enum for {scheme} and {actionType}!");
        }
        return foundType;
    }
    #endregion

    #region GetTypeActions
    public List<Enum> GetTypeActions<TAudit>()
        => GetTypeActions(typeof(TAudit));
    public List<Enum> GetTypeActions(Type actionType)
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
