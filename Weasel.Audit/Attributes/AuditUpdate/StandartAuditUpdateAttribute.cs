using Microsoft.EntityFrameworkCore;

namespace Weasel.Audit.Attributes.AuditUpdate;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class StandartAuditUpdateAttribute : AuditUpdateStrategyAttribute
{
    public override bool Compare(DbContext context, object? old, object? update, object? oldValue, object? updateValue)
    {
        if (ReferenceEquals(oldValue, updateValue))
        {
            return true;
        }

        if (oldValue is null || updateValue is null)
        {
            return false;
        }

        if (oldValue.GetType() == updateValue.GetType())
        {
            return oldValue.Equals(updateValue);
        }

        switch (oldValue)
        {
            case float f1:
                return updateValue switch
                {
                    double d2 => f1 == d2,
                    IConvertible c2 => f1 == c2.ToSingle(null),
                    _ => false,
                };
            case double d1:
                return updateValue is IConvertible conv2
                    ? d1 == conv2.ToDouble(null)
                    : false;

            case IConvertible c1:
                return updateValue switch
                {
                    float f2 => c1.ToSingle(null) == f2,
                    double d2 => c1.ToDouble(null) == d2,
                    IConvertible c2 => c1.ToDecimal(null) == c2.ToDecimal(null),
                    _ => false,
                };
            default:
                return false;
        }
    }
    public override object? SetValue(DbContext context, object? old, object? update, object? oldValue, object? updateValue)
        => updateValue;
}
