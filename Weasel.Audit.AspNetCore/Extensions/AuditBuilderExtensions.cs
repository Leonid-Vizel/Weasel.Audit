using Microsoft.AspNetCore.Builder;
using Weasel.Audit.AspNetCore.Middleware;

namespace Weasel.Audit.AspNetCore.Extensions;

public static class AuditBuilderExtensions
{
    public static IApplicationBuilder UseResponseCompression(this IApplicationBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        return builder.UseMiddleware<PostponedAuditMiddleware>();
    }
}
