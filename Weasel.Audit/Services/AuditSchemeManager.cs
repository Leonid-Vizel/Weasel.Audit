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

public interface IAuditSchemeManager<TEnum, TColor>
	where TEnum : struct, Enum
    where TColor : struct, Enum
{
	AuditDescAttribute? GetAuditEnumDescription(TEnum type);
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
	private readonly ConcurrentDictionary<TEnum, AuditDescAttribute> _enumDescriptions;
    private readonly ConcurrentDictionary<Enum, AuditColorAttribute> _colorDescriptions;
    private readonly ConcurrentDictionary<string, Type> _auditTypeSearchDictionary;
	private readonly ConcurrentDictionary<TypeAuditSchemeKey, TEnum[]> _typeSchemaActions;
	public AuditSchemeManager()
	{
		_colorDescriptions = new ConcurrentDictionary<Enum, AuditColorAttribute>();
        _typeActions = new ConcurrentDictionary<Type, List<TEnum>>();
		_typeSchemaActions = new ConcurrentDictionary<TypeAuditSchemeKey, TEnum[]>();
		_enumDescriptions = new ConcurrentDictionary<TEnum, AuditDescAttribute>();
		_auditTypeSearchDictionary = new ConcurrentDictionary<string, Type>();
		foreach (TEnum type in Enum.GetValues<TEnum>())
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
