using Weasel.Tools.Extensions.Common;

namespace Weasel.Attributes.Audit.Rows;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ActionRowEnumNamingRuleAttribute<T> : ActionRowNamingRuleAttribute where T : struct, Enum
{
    public override string Process(int index)
    {
        T value = (T)(object)index;
        return value.GetDisplayNameNonNull();
    }
}
