using System.Reflection;
using Weasel.Audit.Attributes.Search;
using Weasel.Audit.Enums;

namespace Weasel.Audit.Attributes.Enums;

public abstract class AuditActionDescriptionAttribute : Attribute
{
    public Enum Journal { get; private set; }
    public string Name { get; private set; }
    public Enum Color { get; private set; }
    public AuditScheme Scheme { get; private set; }
    public Type Type { get; private set; }
    public string SearchTypeName { get; private set; }
    public string SearchUrlTypeName { get; private set; }
    public AuditActionDescriptionAttribute(Enum journal, string name, Enum color, AuditScheme scheme, Type type)
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

[AttributeUsage(AttributeTargets.Field)]
public sealed class AuditActionDescriptionAttribute<TJournal, TColor> : AuditActionDescriptionAttribute
    where TJournal : Enum
    where TColor : Enum
{
    public AuditActionDescriptionAttribute(TJournal journal, string name, TColor color, AuditScheme scheme, Type type)
        : base(journal, name, color, scheme, type) { }
}
