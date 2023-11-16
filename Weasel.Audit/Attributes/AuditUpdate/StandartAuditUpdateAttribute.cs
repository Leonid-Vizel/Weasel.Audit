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
                switch (updateValue)
                {
                    case double d2:
                        return f1 == d2;
                    case IConvertible c2:
                        return f1 == c2.ToSingle(null);
                    default:
                        return false;
                }

            case double d1:
                return updateValue is IConvertible conv2
                    ? d1 == conv2.ToDouble(null)
                    : false;

            case IConvertible c1:
                switch (updateValue)
                {
                    case float f2:
                        return c1.ToSingle(null) == f2;
                    case double d2:
                        return c1.ToDouble(null) == d2;
                    case IConvertible c2:
                        return c1.ToDecimal(null) == c2.ToDecimal(null);
                    default:
                        return false;
                }

            default:
                return false;
        }
    }
    public override object? SetValue(DbContext context, object? old, object? update, object? oldValue, object? updateValue)
        => updateValue;
}
