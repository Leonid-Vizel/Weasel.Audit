using Microsoft.AspNetCore.Builder;
using Weasel.Audit.Interfaces;

namespace Weasel.Audit.AspNetCore.Extensions;

public static class AuditBuilderExtensions
{
    public static IApplicationBuilder UseAudit<TAuditAction, TEnum, TColor>(this IApplicationBuilder builder)
        where TAuditAction : class, IAuditAction<TEnum>
		where TEnum : struct, Enum
        where TColor : struct, Enum
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        return builder.UseMiddleware<PostponedAuditMiddleware<TAuditAction, TEnum, TColor>>();
    }
}
