using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Models;
using Weasel.Enums;
using Weasel.Tools.Extensions.Common;

namespace Weasel.Audit.Services;

public interface IAuditPropertyManager
{
    IAuditPropertyStorage Storage { get; }
    Func<object, object> CreatePropertyGetter(PropertyInfo info);
    Action<object, object> CreatePropertySetter(PropertyInfo info);
    void PerformCustomUpdate<T>(DbContext context, T old, T update) where T : ICustomUpdatable<T>;
    void PerformAutoUpdate<T>(DbContext context, T old, T update);
    ActionIndexItem[] GetEntityDisplayData(Type type, object? model);
}
public sealed class AuditPropertyManager : IAuditPropertyManager
{
    public static readonly List<Type> FieldTypes = new List<Type>()
    {
        typeof(int),
        typeof(int?),
        typeof(long),
        typeof(long?),
        typeof(uint),
        typeof(uint?),
        typeof(ulong),
        typeof(ulong?),
        typeof(byte),
        typeof(byte?),
        typeof(sbyte),
        typeof(sbyte?),
        typeof(short),
        typeof(short?),
        typeof(bool),
        typeof(bool?),
        typeof(float),
        typeof(float?),
        typeof(double),
        typeof(double?),
        typeof(decimal),
        typeof(decimal?),
        typeof(string),
        typeof(char),
        typeof(char?),
        typeof(DateTime),
        typeof(DateTime?),
        typeof(DateOnly),
        typeof(DateOnly?),
        typeof(TimeOnly),
        typeof(TimeOnly?),
    };
    public IAuditPropertyStorage Storage { get; private set; }
    public AuditPropertyManager(IAuditPropertyStorage storage)
    {
        Storage = storage;
    }

    //Just my interpretation of https://stackoverflow.com/questions/17660097/is-it-possible-to-speed-this-method-up/17669142#17669142
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
    public void PerformCustomUpdate<T>(DbContext context, T old, T update) where T : ICustomUpdatable<T>
        => old.Update(update, context);
    public void PerformAutoUpdate<T>(DbContext context, T old, T update)
    {
        if (old == null || update == null)
        {
            return;
        }
        var props = Storage.GetAuditPropertyData(this, typeof(T)).Where(x => x.AutoUpdate).ToArray();
        foreach (var prop in props)
        {
            object? oldValue = prop.Getter.Invoke(old);
            object? updateValue = prop.Getter.Invoke(update);
            if (prop.CompareDelegate?.Invoke(context, old, update, oldValue, updateValue) ?? false)
            {
                object? setValue = prop.SetValueDelegate?.Invoke(context, old, update, oldValue, updateValue) ?? updateValue;
                prop.Setter.Invoke(old, setValue);
            }
        }
    }
    public ActionIndexItem[] GetEntityDisplayData(Type type, object? model)
    {
        var props = Storage.GetAuditPropertyData(this, type).Where(x => x.DisplayMode != AuditPropertyDisplayMode.None).ToArray();
        ActionIndexItem[] items = new ActionIndexItem[props.Length];
        for (int i = 0; i < props.Length; i++)
        {
            var prop = props[i];
            object? value = model == null ? null : prop.Getter.Invoke(model);
            object? processedValue = ProcessValue(prop, value);
            if (prop.ValueFormatter != null)
            {
                processedValue = prop.ValueFormatter.FormatValue(processedValue);
            }
            var item = new ActionIndexItem()
            {
                Value = processedValue,
                Name = prop.Name,
            };
            items[i] = item;
        }
        return items;
    }
    private object? ProcessValue(AuditPropertyCache prop, object? value)
    {
        switch (prop.DisplayMode)
        {
            case AuditPropertyDisplayMode.List:
                IList? innerValues = value as IList;
                if (innerValues == null || prop.InnerListType == null || innerValues.Count == 0)
                {
                    return new ActionIndexItem[0];
                }
                ActionIndexItem[] items = new ActionIndexItem[innerValues.Count];
                int counter = 0;
                for (int i = 0; i < innerValues.Count; i++)
                {
                    var innerValue = innerValues[i];
                    var name = prop.RowNaming?.Process(counter) ?? $"Строка #{counter + 1}";
                    counter++;
                    items[i] = new ActionIndexItem(name)
                    {
                        Value = innerValue != null ? GetEntityDisplayData(prop.InnerListType, innerValue) : null
                    };
                }
                return items;
            case AuditPropertyDisplayMode.Object:
                if (value == null)
                {
                    return new ActionIndexItem[0];
                }
                return GetEntityDisplayData(prop.Type, value);
            case AuditPropertyDisplayMode.Field:
                return value;
            case AuditPropertyDisplayMode.Enum:
                return (value as Enum)?.GetDisplayName() ?? null;
            default:
                return new ActionIndexItem[0];
        }
    }
}
