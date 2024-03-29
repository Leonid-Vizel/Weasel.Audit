﻿using Microsoft.AspNetCore.Http;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Services;

namespace Weasel.Audit.AspNetCore.Extensions;

public sealed class PostponedAuditMiddleware<TAction, TRow, TEnum, TColor>
    where TAction : class, IAuditAction<TRow, TEnum>
    where TRow : IAuditRow<TEnum>
    where TColor : struct, Enum
	where TEnum : struct, Enum
{
    private readonly RequestDelegate _next;
    public PostponedAuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IPostponedAuditManager<TAction, TRow, TEnum, TColor> manager)
    {
        await _next(context);
        manager.ExecuteAndDispose();
    }
}
