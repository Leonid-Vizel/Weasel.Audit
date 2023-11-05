using System.Linq.Expressions;
using System.Reflection;
using Weasel.Attributes.Audit.Formatters;
using Weasel.Attributes.Audit.Rows;
using Weasel.Enums;

namespace Weasel.Services.Audit;

public struct AuditPropertyCache
{
    public Type Type { get; private set; } = null!;
    public Func<object, object> Getter { get; private set; } = null!;
    public Action<object, object> Setter { get; private set; } = null!;
    public AuditValueFormatterAttribute? ValueFormatter { get; private set; }
    public AuditRowNamingRuleAttribute? RowNaming { get; private set; }
    public AuditPropertyDisplayMode DisplayMode { get; private set; }
}

public sealed class AuditPropertyManager
{
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
