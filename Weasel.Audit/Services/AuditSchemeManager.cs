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
    private readonly ConcurrentDictionary<Enum, AuditActionDescriptionAttribute> _enumDescriptions;
    private readonly ConcurrentDictionary<TypeAuditSchemeKey, Enum[]> _typeSchemaActions;
    public AuditSchemeManager(Type[] enumTypes)
    {
        _typeActions = new ConcurrentDictionary<Type, List<Enum>>();
        _typeSchemaActions = new ConcurrentDictionary<TypeAuditSchemeKey, Enum[]>();
        _enumDescriptions = new ConcurrentDictionary<Enum, AuditActionDescriptionAttribute>();
        foreach (var enumType in enumTypes.Where(x => x.IsEnum))
        {
            foreach (Enum type in Enum.GetValues(enumType))
            {
                var decription = GetAuditEnumDescription(type);
                if (decription == null)
                {
                    throw new ArgumentNullException($"Provide {nameof(AuditActionDescriptionAttribute)} attribute for {type}!");
                }
                _enumDescriptions.TryAdd(type, decription);
            }
        }
    }

    private AuditActionDescriptionAttribute? GetAuditEnumDescription(Enum type)
        => type.GetType().GetMember(type.ToString()).FirstOrDefault()?.GetCustomAttribute<AuditActionDescriptionAttribute>();

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
