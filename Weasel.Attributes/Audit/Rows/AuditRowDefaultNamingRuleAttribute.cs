namespace Weasel.Attributes.Audit.Rows;

[AttributeUsage(AttributeTargets.Property)]
public sealed class AuditRowDefaultNamingRuleAttribute : AuditRowNamingRuleAttribute
{
    public string Name { get; set; }
    public string Separator { get; set; }
    public int IndexOffset { get; set; }
    public AuditRowDefaultNamingRuleAttribute(string name = "Строка", string separator = "#", int indexOffset = 1)
    {
        Name = name;
        Separator = separator;
        IndexOffset = indexOffset;
    }
    public override string Process(int index)
        => $"{Name} {Separator}{index + IndexOffset}";
}
