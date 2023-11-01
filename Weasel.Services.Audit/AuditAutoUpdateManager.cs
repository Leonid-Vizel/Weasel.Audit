using System.Linq.Expressions;
using System.Reflection;

namespace Weasel.Services.Audit;

public sealed class AuditAutoUpdateManager
{
    //Just my interpretation of https://stackoverflow.com/questions/17660097/is-it-possible-to-speed-this-method-up/17669142#17669142
    public static Func<object, object> CreatePropertyGetter(PropertyInfo info)
    {
        var exInstance = Expression.Parameter(info.DeclaringType, "t");
        var exMemberAccess = Expression.MakeMemberAccess(exInstance, info);
        var exConvertToObject = Expression.Convert(exMemberAccess, typeof(object));
        var lambda = Expression.Lambda<Func<object, object>>(exConvertToObject, exInstance);
        return lambda.Compile();
    }

    public static Action<object, object> CreatePropertySetter(PropertyInfo info)
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
