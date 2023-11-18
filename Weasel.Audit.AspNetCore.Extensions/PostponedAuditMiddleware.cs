using Microsoft.AspNetCore.Http;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Services;

namespace Weasel.Audit.AspNetCore.Extensions;

public sealed class PostponedAuditMiddleware<TAuditAction>
    where TAuditAction : class, IAuditAction
{
    private readonly RequestDelegate _next;
    public PostponedAuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IPostponedAuditManager<TAuditAction> manager)
    {
        await _next(context);
        manager.ExecuteAndDispose();
    }
}
