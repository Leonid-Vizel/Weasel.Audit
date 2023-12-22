using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using Weasel.Audit.Attributes.Display;
using Weasel.Audit.Enums;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Models;

namespace Weasel.Audit.Services;

public interface IAuditPropertyManager
{
    IAuditPropertyStorage Storage { get; }
    Func<object, object>? CreatePropertyGetter(PropertyInfo info);
    Action<object, object>? CreatePropertySetter(PropertyInfo info);
    void PerformUpdate<T>(DbContext context, T old, T update);
    void PerformUpdateRange<T>(DbContext context, IReadOnlyList<Tuple<T, T>> updateData);
    List<AuditPropertyDisplayModel> GetEntityDisplayData(Type type, object? model);
}
public sealed class AuditPropertyManager : IAuditPropertyManager
{
    private static readonly Type objectType = typeof(object);
    public IAuditPropertyStorage Storage { get; private set; }
    public AuditPropertyManager(IAuditPropertyStorage storage)
    {
        Storage = storage;
    }

    [RequiresDynamicCode("Not AOT friendly: System.Reflection.Emit is used")]
    public Func<object, object>? CreatePropertyGetter(PropertyInfo info)
    {
        if (info.DeclaringType == null)
        {
            return null;
        }
        var getMethod = info.GetGetMethod();
        if (getMethod == null)
        {
            return null;
        }
        DynamicMethod method = new DynamicMethod("PropertyGetter", objectType, [objectType], Assembly.GetExecutingAssembly().ManifestModule);
        ILGenerator il = method.GetILGenerator(100);

        il.Emit(OpCodes.Ldarg_0);
        il.EmitCall(OpCodes.Callvirt, getMethod, null);
        if (info.PropertyType.IsValueType)
        {
            il.Emit(OpCodes.Box, info.PropertyType);
        }
        il.Emit(OpCodes.Ret);

        return (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
    }
    [RequiresDynamicCode("Not AOT friendly: System.Reflection.Emit is used")]
    public Action<object, object>? CreatePropertySetter(PropertyInfo info)
    {
        if (info.DeclaringType == null)
        {
            return null;
        }
        var setMethod = info.GetSetMethod();
        if (setMethod == null)
        {
            return null;
        }
        DynamicMethod method = new DynamicMethod("PropertySetter", null, [typeof(object), typeof(object)], Assembly.GetExecutingAssembly().ManifestModule);
        ILGenerator il = method.GetILGenerator();

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Castclass, info.DeclaringType);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(info.PropertyType.IsClass ? OpCodes.Castclass : OpCodes.Unbox_Any, info.PropertyType);
        il.EmitCall(OpCodes.Callvirt, setMethod, null);
        il.Emit(OpCodes.Ret);

        return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
    }
    public void PerformUpdate<T>(DbContext context, T old, T update)
    {
        ArgumentNullException.ThrowIfNull(context);
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
            if (prop.Getter == null || prop.Setter == null)
            {
                continue;
            }
            object? oldValue = prop.Getter.Invoke(old);
            object? updateValue = prop.Getter.Invoke(update);
            if (!prop.UpdateStrategy.Compare(context, old, update, oldValue, updateValue))
            {
                object? setValue = prop.UpdateStrategy.SetValue(context, old, update, oldValue, updateValue) ?? updateValue;
                prop.Setter.Invoke(old, setValue);
            }
        }
    }
    public void PerformUpdateRange<T>(DbContext context, IReadOnlyList<Tuple<T, T>> updateData)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(updateData);
        bool performCustom = typeof(T).IsAssignableTo(typeof(ICustomUpdatable<T>));
        var props = Storage.GetAuditPropertyData(this, typeof(T));
        foreach (var pair in updateData)
        {
            var old = pair.Item1;
            var update = pair.Item2;
            if (old == null)
            {
                throw new Exception("\"old\" value should not be null!");
            }
            if (update == null)
            {
                throw new Exception("\"update\" value should not be null!");
            }
            if (performCustom)
            {
                (old as ICustomUpdatable<T>)?.Update(update, context);
            }
            foreach (var prop in props)
            {
                if (prop.Getter == null || prop.Setter == null)
                {
                    continue;
                }
                object? oldValue = prop.Getter.Invoke(old);
                object? updateValue = prop.Getter.Invoke(update);
                if (!prop.UpdateStrategy.Compare(context, old, update, oldValue, updateValue))
                {
                    object? setValue = prop.UpdateStrategy.SetValue(context, old, update, oldValue, updateValue) ?? updateValue;
                    prop.Setter.Invoke(old, setValue);
                }
            }
        }
    }
    private object? GetPropertyDisplayModels(AuditPropertyCache prop, object? declare, object? value, AuditPropertyDisplayMode mode)
    {
        switch (mode)
        {
            case AuditPropertyDisplayMode.Collection:
                if (value is not ICollection values || values.Count == 0)
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
            case AuditPropertyDisplayMode.SingularRelation:
                if (prop.DisplayStrategy is not SingularRelationDisplayAttribute relation)
                {
                    return new List<AuditPropertyDisplayModel>();
                }
                return new AuditRelationDisplayModel(prop.Name, relation.RelatingType, value);
            default:
                return new List<AuditPropertyDisplayModel>();
        }
    }
    public List<AuditPropertyDisplayModel> GetEntityDisplayData(Type type, object? model)
    {
        var props = Storage.GetAuditPropertyData(this, type);
        var items = new List<AuditPropertyDisplayModel>();
        foreach (var prop in props)
        {
            if (prop.Getter == null)
            {
                continue;
            }
            object? value = model == null ? null : prop.Getter.Invoke(model);
            object? formattedValue = prop.DisplayStrategy.FormatValue(prop.Info, model, value);
            var mode = prop.GetDisplayMode(model, formattedValue);
            if (mode == AuditPropertyDisplayMode.None)
            {
                continue;
            }
            items.Add(new AuditPropertyDisplayModel()
            {
                Value = GetPropertyDisplayModels(prop, model, formattedValue, mode),
                Name = prop.Name,
            });
        }
        return items;
    }
}
