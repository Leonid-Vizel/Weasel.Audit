using Microsoft.AspNetCore.Builder;
using Weasel.Audit.Interfaces;

namespace Weasel.Audit.AspNetCore.Extensions;

public static class AuditBuilderExtensions
{
    public static IApplicationBuilder UseAudit<TAuditAction>(this IApplicationBuilder builder)
        where TAuditAction : class, IAuditAction
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        return builder.UseMiddleware<PostponedAuditMiddleware<TAuditAction>>();
    }
}
