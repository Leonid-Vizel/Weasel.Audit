namespace Weasel.Audit.Attributes.Rows;

[AttributeUsage(AttributeTargets.Property)]
public abstract class AuditRowNamingRuleAttribute : Attribute
{
    public abstract string Process(int index);
}
