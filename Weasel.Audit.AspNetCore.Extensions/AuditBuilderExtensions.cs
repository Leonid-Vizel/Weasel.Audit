using Microsoft.AspNetCore.Builder;

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
