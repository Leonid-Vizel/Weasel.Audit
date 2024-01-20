using Microsoft.AspNetCore.Builder;
using Weasel.Audit.Interfaces;

namespace Weasel.Audit.AspNetCore.Extensions;

public static class AuditBuilderExtensions
{
    public static IApplicationBuilder UseAudit<TAction, TRow, TEnum, TColor>(this IApplicationBuilder builder)
        where TAction : class, IAuditAction<TRow, TEnum>
        where TRow : IAuditRow<TEnum>
        where TColor : struct, Enum
		where TEnum : struct, Enum
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        return builder.UseMiddleware<PostponedAuditMiddleware<TAction, TRow, TEnum, TColor>>();
    }
}
