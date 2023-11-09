using Microsoft.EntityFrameworkCore;

namespace Weasel.Audit.Attributes.AutoUpdate.Strategy;

public sealed class StandartAutoUpdateStrategyAttribbute : AutoUpdateStrategyAttribute
{
    //https://stackoverflow.com/questixons/52078501/comparing-boxed-values-of-different-types

    /// <summary>
    /// Standart Compare implementation
    /// </summary>
    /// <param name="context">JUST TO FIT AutoUpdateStrategyAttribute.CompareDelegate</param>
    /// <param name="old">JUST TO FIT AutoUpdateStrategyAttribute.CompareDelegate</param>
    /// <param name="update">JUST TO FIT AutoUpdateStrategyAttribute.CompareDelegate</param>
    /// <param name="oldValue">Old value</param>
    /// <param name="newValue">Update Value</param>
    /// <returns><see langword="true"/> if <paramref name="oldValue"/> equals <paramref name="newValue"/></returns>
    public static bool CompareBoxedValues(DbContext context, object? old, object? update, object? oldValue, object? newValue)
    {
        if (ReferenceEquals(oldValue, newValue))
        {
            return true;
        }

        if (oldValue is null || newValue is null)
        {
            return false;
        }

        if (oldValue.GetType() == newValue.GetType())
        {
            return oldValue.Equals(newValue);
        }

        switch (oldValue)
        {
            case float f1:
                switch (newValue)
                {
                    case double d2:
                        return f1 == d2;
                    case IConvertible c2:
                        return f1 == c2.ToSingle(null);
                    default:
                        return false;
                }

            case double d1:
                return newValue is IConvertible conv2
                    ? d1 == conv2.ToDouble(null)
                    : false;

            case IConvertible c1:
                switch (newValue)
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
    public override bool Compare(DbContext context, object? old, object? update, object? oldValue, object? updateValue)
        => CompareBoxedValues(context, old, update, oldValue, updateValue);

    /// <summary>
    /// Standart SetValue implementation
    /// </summary>
    /// <param name="context">JUST TO FIT AutoUpdateStrategyAttribute.SetValueDelegate</param>
    /// <param name="old">JUST TO FIT AutoUpdateStrategyAttribute.SetValueDelegate</param>
    /// <param name="update">JUST TO FIT AutoUpdateStrategyAttribute.SetValueDelegate</param>
    /// <param name="oldValue">JUST TO FIT AutoUpdateStrategyAttribute.SetValueDelegate</param>
    /// <param name="newValue">Value that should be returned</param>
    /// <returns><paramref name="newValue"/></returns>
    public static object? StandartSetValue(DbContext context, object? old, object? update, object? oldValue, object? newValue)
        => newValue;
    public override object? SetValue(DbContext context, object? old, object? update, object? oldValue, object? newValue)
        => StandartSetValue(context, old, update, oldValue, newValue);
}
