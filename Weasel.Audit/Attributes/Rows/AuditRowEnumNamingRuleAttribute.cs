using Weasel.Tools.Extensions.Common;

namespace Weasel.Audit.Attributes.Rows;

[AttributeUsage(AttributeTargets.Property)]
public sealed class AuditRowEnumNamingRuleAttribute<T> : AuditRowNamingRuleAttribute where T : struct, Enum
{
    public override string Process(int index)
    {
        T value = (T)(object)index;
        return value.GetDisplayNameNonNull();
    }
}
