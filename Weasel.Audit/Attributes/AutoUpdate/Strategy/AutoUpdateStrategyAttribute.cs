using Microsoft.EntityFrameworkCore;

namespace Weasel.Audit.Attributes.AutoUpdate.Strategy;

public abstract class AutoUpdateStrategyAttribute : Attribute
{
    /// <summary>
    /// Comparing method
    /// </summary>
    /// <returns><see langword="true"/> - if equal; <see langword="false"/> - if different</returns>
    public abstract bool Compare(DbContext context, object? old, object? update, object? oldValue, object? updateValue);

    /// <summary>
    /// Setting method
    /// </summary>
    /// <returns>Your value boxed in <see langword="object"/> that should be set to the field if Compare method returns <see langword="false"/></returns>
    public abstract object? SetValue(DbContext context, object? old, object? update, object? oldValue, object? newValue);

    public delegate bool CompareDelegate(DbContext context, object? old, object? update, object? oldValue, object? updateValue);
    public delegate object? SetValueDelegate(DbContext context, object? old, object? update, object? oldValue, object? updateValue);
}
