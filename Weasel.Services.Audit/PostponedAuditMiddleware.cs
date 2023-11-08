using Microsoft.AspNetCore.Http;

namespace Weasel.Services.Audit;

public sealed class PostponedAuditMiddleware
{
    private readonly RequestDelegate _next;
    public PostponedAuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IPostponedAuditManager manager)
    {
        await _next(context);
        #pragma warning disable CS4014
        manager.ExecuteAndDispose();
        #pragma warning restore CS4014
    }
}
