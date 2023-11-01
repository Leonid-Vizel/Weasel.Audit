namespace Weasel.Attributes.Audit.Rows;

[AttributeUsage(AttributeTargets.Property)]
public abstract class ActionRowNamingRuleAttribute : Attribute
{
    public abstract string Process(int index);
}
