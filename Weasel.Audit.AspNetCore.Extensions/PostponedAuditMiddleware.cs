using Microsoft.AspNetCore.Http;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Services;

namespace Weasel.Audit.AspNetCore.Extensions;

public sealed class PostponedAuditMiddleware<TAuditAction, TEnum>
    where TAuditAction : class, IAuditAction<TEnum>
	where TEnum : struct, Enum
{
    private readonly RequestDelegate _next;
    public PostponedAuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IPostponedAuditManager<TAuditAction, TEnum> manager)
    {
        await _next(context);
        manager.ExecuteAndDispose();
    }
}
