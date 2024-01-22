using Weasel.Audit.Enums;

namespace Weasel.Audit.Attributes.Enums;

public abstract class AuditDescAttribute : Attribute
{
    public Enum Journal { get; private set; }
    public string Name { get; private set; }
    public Enum Color { get; private set; }
    public AuditScheme Scheme { get; private set; }
    public Type Type { get; private set; }
    public string SearchTypeName { get; private set; }
    public AuditDescAttribute(Enum journal, string name, Enum color, AuditScheme scheme, Type type)
    {
        Journal = journal;
        Name = name;
        Color = color;
        Scheme = scheme;
        Type = type;
        SearchTypeName = Type.Name.ToLower();
    }
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class AuditDescAttribute<TJournal, TColor> : AuditDescAttribute
    where TJournal : Enum
    where TColor : Enum
{
    public AuditDescAttribute(TJournal journal, string name, TColor color, AuditScheme scheme, Type type)
        : base(journal, name, color, scheme, type) { }
}
