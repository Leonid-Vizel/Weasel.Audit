using System.Linq.Expressions;
using System.Reflection;

namespace Weasel.Services.Audit;

public interface IAuditPropertyManager
{
    Func<object, object> CreatePropertyGetter(PropertyInfo info);
    Action<object, object> CreatePropertySetter(PropertyInfo info);
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
    //Just my interpretation of https://stackoverflow.com/questions/17660097/is-it-possible-to-speed-this-method-up/17669142#17669142
    public Func<object, object> CreatePropertyGetter(PropertyInfo info)
    {
        var exInstance = Expression.Parameter(info.DeclaringType, "t");
        var exMemberAccess = Expression.MakeMemberAccess(exInstance, info);
        var exConvertToObject = Expression.Convert(exMemberAccess, typeof(object));
        var lambda = Expression.Lambda<Func<object, object>>(exConvertToObject, exInstance);
        return lambda.Compile();
    }
    public Action<object, object> CreatePropertySetter(PropertyInfo info)
    {
        var exInstance = Expression.Parameter(info.DeclaringType, "t");
        var exMemberAccess = Expression.MakeMemberAccess(exInstance, info);
        var exValue = Expression.Parameter(typeof(object), "p");
        var exConvertedValue = Expression.Convert(exValue, info.PropertyType);
        var exBody = Expression.Assign(exMemberAccess, exConvertedValue);
        var lambda = Expression.Lambda<Action<object, object>>(exBody, exInstance, exValue);
        return lambda.Compile();
    }
}
