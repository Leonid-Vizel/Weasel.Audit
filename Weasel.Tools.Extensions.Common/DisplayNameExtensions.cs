using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Weasel.Tools.Extensions.Common;

[AttributeUsage(AttributeTargets.Field)]
public sealed class EnumGroupingAttribute : Attribute
{
    public string[] Grouping { get; set; }
    public EnumGroupingAttribute(params string[] grouping)
    {
        Grouping = grouping;
    }
}

public static class DisplayNameExtensions
{
    private static ConcurrentDictionary<Enum, string?> _enumNamesData;
    private static ConcurrentDictionary<Enum, string[]?> _enumGroupingsData;
    private static ConcurrentDictionary<(Type, string), string?> _propertyNamesData;
    private static ConcurrentDictionary<Type, string?> _typeNamesData;
    static DisplayNameExtensions()
    {
        _enumNamesData = new ConcurrentDictionary<Enum, string?>();
        _enumGroupingsData = new ConcurrentDictionary<Enum, string[]?>();
        _propertyNamesData = new ConcurrentDictionary<(Type, string), string?>();
        _typeNamesData = new ConcurrentDictionary<Type, string?>();
    }

    #region Enums
    public static string? GetDisplayName(this Enum enumValue)
    {
        if (!_enumNamesData.TryGetValue(enumValue, out var result))
        {
            result = enumValue.GetType()
                        .GetMember(enumValue.ToString())
                        .FirstOrDefault(x=>x.DeclaringType?.IsEnum ?? false)?
                        .GetCustomAttribute<DisplayAttribute>()?
                        .GetName();
            _enumNamesData.TryAdd(enumValue, result);
        }
        return result;
    }
    public static string[]? GetGroupings(this Enum enumValue)
    {
        if (!_enumGroupingsData.TryGetValue(enumValue, out var result))
        {
            result = enumValue.GetType()
                        .GetMember(enumValue.ToString())
                        .FirstOrDefault()?
                        .GetCustomAttribute<EnumGroupingAttribute>()?
                        .Grouping;
            _enumGroupingsData.TryAdd(enumValue, result);
        }
        return result;
    }
    public static string GetDisplayNameNonNull(this Enum enumValue, string nullValue = "")
        => enumValue.GetDisplayName() ?? nullValue;
    #endregion

    #region Properties
    public static string? GetDisplayName(this object obj, string propertyName)
        => obj.GetType().GetDisplayName(propertyName);

    public static string GetDisplayNameNonNull(this object obj, string propertyName, string nullValue = "")
        => obj.GetType().GetDisplayName(propertyName) ?? nullValue;

    public static string? GetDisplayName<T>(string propertyName)
        => typeof(T).GetDisplayName(propertyName);

    public static string? GetDisplayName<T>(Expression<Func<T, object?>> lambda)
        => lambda.GetPropertyInfo()?.GetDisplayName();

    public static string GetDisplayNameNonNull<T>(string propertyName, string nullValue = "")
        => typeof(T).GetDisplayName(propertyName) ?? nullValue;

    public static string GetDisplayNameNonNull<T>(Expression<Func<T, object?>> lambda, string nullValue = "")
        => lambda.GetPropertyInfo()?.GetDisplayName() ?? nullValue;

    public static string? GetDisplayName(this Type objType, string propertyName)
    {
        var key = (objType, propertyName);
        if (!_propertyNamesData.TryGetValue(key, out string? name))
        {
            var propInfo = objType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            if (propInfo == null)
            {
                propInfo = objType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
                if (propInfo == null)
                {
                    throw new Exception($"Property named {propertyName} not found in {objType.Name}!");
                }
            }
            name = propInfo.GetDisplayName();
            _propertyNamesData.TryAdd(key, name);
        }
        return name;
    }

    public static PropertyInfo? GetPropertyInfo<T, TValue>(this Expression<Func<T, TValue>> lambda)
    {
        UnaryExpression? unaryExpression = lambda.Body as UnaryExpression;
        MemberExpression? memberExpression = null;
        if (unaryExpression != null)
        {
            memberExpression = unaryExpression.Operand as MemberExpression;
        }
        else
        {
            memberExpression = lambda.Body as MemberExpression;
        }
        return memberExpression?.Member as PropertyInfo;
    }

    public static string? GetDisplayName(this PropertyInfo propInfo)
        => propInfo.GetCustomAttributes<DisplayNameAttribute>()?.FirstOrDefault()?.DisplayName;

    public static string GetDisplayNameNonNull(this PropertyInfo propInfo, string nullValue = "")
        => propInfo.GetDisplayName() ?? nullValue;
    #endregion

    #region Types
    public static string? GetDisplayName(this object obj)
        => obj.GetType().GetDisplayName();

    public static string? GetDisplayName<T>()
        => typeof(T).GetDisplayName();

    public static string? GetDisplayName(this Type objType)
    {
        if (!_typeNamesData.TryGetValue(objType, out string? name))
        {
            name = objType.GetCustomAttributes<DisplayNameAttribute>()?.FirstOrDefault()?.DisplayName;
            _typeNamesData.TryAdd(objType, name);
        }
        return name;
    }
    #endregion
}
