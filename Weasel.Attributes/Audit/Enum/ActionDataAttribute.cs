using System.Reflection;
using Weasel.Attributes.Audit.Search;
using Weasel.Enums;

namespace ECRF.Tools.Actions.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class ActionDataAttribute : Attribute
{
    public Enum Journal { get; private set; }
    public string Name { get; private set; }
    public Enum Color { get; private set; }
    public AuditScheme Scheme { get; private set; }
    public Type Type { get; private set; }
    public string SearchTypeName { get; private set; }
    public string SearchUrlTypeName { get; private set; }
    public ActionDataAttribute(Enum journal, string name, Enum color, AuditScheme scheme, Type type)
    {
        Journal = journal;
        Name = name;
        Color = color;
        Scheme = scheme;
        Type = type;
        var customName = Type.GetCustomAttribute<AuditCustomSearchAttribute>();
        SearchTypeName = customName?.SearchName ?? Type.Name.ToLower();
        SearchUrlTypeName = customName?.SearchUrlName ?? Type.Name;
    }
}
