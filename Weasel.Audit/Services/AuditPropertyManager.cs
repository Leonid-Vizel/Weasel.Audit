using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Weasel.Audit.Enums;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Models;

namespace Weasel.Audit.Services;

public interface IAuditPropertyManager
{
    IAuditPropertyStorage Storage { get; }
    Func<object, object> CreatePropertyGetter(PropertyInfo info);
    Action<object, object> CreatePropertySetter(PropertyInfo info);
    void PerformUpdate<T>(DbContext context, T old, T update);
    void PerformUpdateRange<T>(DbContext context, IReadOnlyList<Tuple<T, T>> updateData);
    List<AuditPropertyDisplayModel> GetEntityDisplayData(Type type, object? model);
}
public sealed class AuditPropertyManager : IAuditPropertyManager
{
    public IAuditPropertyStorage Storage { get; private set; }
    public AuditPropertyManager(IAuditPropertyStorage storage)
    {
        Storage = storage;
    }
    public Func<object, object> CreatePropertyGetter(PropertyInfo info)
    {
        if (info.DeclaringType == null)
        {
            throw new Exception($"DeclaringType of PropertyInfo is NULL. VB.NET modules are not supported by this library!");
        }
        var exInstance = Expression.Parameter(info.DeclaringType, "t");
        var exMemberAccess = Expression.MakeMemberAccess(exInstance, info);
        var exConvertToObject = Expression.Convert(exMemberAccess, typeof(object));
        var lambda = Expression.Lambda<Func<object, object>>(exConvertToObject, exInstance);
        return lambda.Compile();
    }
    public Action<object, object> CreatePropertySetter(PropertyInfo info)
    {
        if (info.DeclaringType == null)
        {
            throw new Exception($"DeclaringType of PropertyInfo is NULL. VB.NET modules are not supported by this library!");
        }
        var exInstance = Expression.Parameter(info.DeclaringType, "t");
        var exMemberAccess = Expression.MakeMemberAccess(exInstance, info);
        var exValue = Expression.Parameter(typeof(object), "p");
        var exConvertedValue = Expression.Convert(exValue, info.PropertyType);
        var exBody = Expression.Assign(exMemberAccess, exConvertedValue);
        var lambda = Expression.Lambda<Action<object, object>>(exBody, exInstance, exValue);
        return lambda.Compile();
    }
    public void PerformUpdate<T>(DbContext context, T old, T update)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        if (old == null)
        {
            throw new ArgumentNullException(nameof(old));
        }
        if (update == null)
        {
            throw new ArgumentNullException(nameof(update));
        }
        if (old is ICustomUpdatable<T> oldUpdatable)
        {
            oldUpdatable.Update(update, context);
        }
        var props = Storage.GetAuditPropertyData(this, typeof(T));
        foreach (var prop in props)
        {
            object? oldValue = prop.Getter.Invoke(old);
            object? updateValue = prop.Getter.Invoke(update);
            if (prop.UpdateStrategy.Compare(context, old, update, oldValue, updateValue))
            {
                object? setValue = prop.UpdateStrategy.SetValue(context, old, update, oldValue, updateValue) ?? updateValue;
                prop.Setter.Invoke(old, setValue);
            }
        }
    }
    public void PerformUpdateRange<T>(DbContext context, IReadOnlyList<Tuple<T, T>> updateData)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        if (updateData == null)
        {
            throw new ArgumentNullException(nameof(updateData));
        }
        bool performCustom = typeof(T).IsAssignableTo(typeof(ICustomUpdatable<T>));
        var props = Storage.GetAuditPropertyData(this, typeof(T));
        foreach (var pair in updateData)
        {
            var old = pair.Item1;
            var update = pair.Item2;
            if (old == null)
            {
                throw new ArgumentNullException(nameof(old));
            }
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }
            if (performCustom)
            {
                (old as ICustomUpdatable<T>)?.Update(update, context);
            }
            foreach (var prop in props)
            {
                object? oldValue = prop.Getter.Invoke(old);
                object? updateValue = prop.Getter.Invoke(update);
                if (prop.UpdateStrategy.Compare(context, old, update, oldValue, updateValue))
                {
                    object? setValue = prop.UpdateStrategy.SetValue(context, old, update, oldValue, updateValue) ?? updateValue;
                    prop.Setter.Invoke(old, setValue);
                }
            }
        }
    }
    private object? GetPropertyDisplayModels(AuditPropertyCache prop, object? declare, object? value)
    {
        var mode = prop.GetDisplayMode(declare, value);
        switch (mode)
        {
            case AuditPropertyDisplayMode.Collection:
                ICollection? values = value as ICollection;
                if (values == null || values.Count == 0)
                {
                    return new List<AuditPropertyDisplayModel>();
                }
                var models = new List<AuditPropertyDisplayModel>();
                int index = 0;
                foreach (var collection in values)
                {
                    var name = prop.GetRowName(index++, declare, value);
                    models.Add(new AuditPropertyDisplayModel()
                    {
                        Name = prop.GetRowName(index++, declare, value),
                        Value = GetEntityDisplayData(prop.Info.PropertyType, value)
                    });
                }
                return models;
            case AuditPropertyDisplayMode.Object:
                if (value == null)
                {
                    return new List<AuditPropertyDisplayModel>();
                }
                return GetEntityDisplayData(prop.Info.PropertyType, value);
            case AuditPropertyDisplayMode.Field:
                return value;
            default:
                return new List<AuditPropertyDisplayModel>();
        }
    }
    public List<AuditPropertyDisplayModel> GetEntityDisplayData(Type type, object? model)
    {
        var props = Storage.GetAuditPropertyData(this, type);
        List<AuditPropertyDisplayModel> items = new List<AuditPropertyDisplayModel>();
        foreach (var prop in props)
        {
            object? value = model == null ? null : prop.Getter.Invoke(model);
            object? formattedValue = prop.DisplayStrategy.FormatValue(prop.Info, model, value);
            if (prop.GetDisplayMode(model, formattedValue) == AuditPropertyDisplayMode.None)
            {
                continue;
            }
            items.Add(new AuditPropertyDisplayModel()
            {
                Value = GetPropertyDisplayModels(prop, model, formattedValue),
                Name = prop.Name,
            });
        }
        return items;
    }
}
