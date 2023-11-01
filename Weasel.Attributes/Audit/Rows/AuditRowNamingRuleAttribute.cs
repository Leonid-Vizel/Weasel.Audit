namespace Weasel.Attributes.Audit.Rows;

[AttributeUsage(AttributeTargets.Property)]
public abstract class AuditRowNamingRuleAttribute : Attribute
{
    public abstract string Process(int index);
}
