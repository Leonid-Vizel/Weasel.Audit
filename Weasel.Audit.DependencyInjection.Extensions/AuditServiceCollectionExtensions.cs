using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Services;

namespace Weasel.Audit.DependencyInjection.Extensions;

public static class AuditServiceCollectionExtensions
{
    public static IServiceCollection AddAudit<TDbContext, TRowFactory, TActionFactory, TAction, TRow, TEnum, TColor>(this IServiceCollection services)
        where TActionFactory : class, IAuditActionFactory<TAction, TRow, TEnum>
        where TRowFactory : class, IAuditRowFactory<TRow, TEnum>
        where TAction : class, IAuditAction<TRow, TEnum>
        where TRow : IAuditRow<TEnum>
        where TDbContext : DbContext
        where TColor : struct, Enum
		where TEnum : struct, Enum
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        services.AddSingleton<IAuditPropertyStorage, AuditPropertyStorage>();
        services.AddSingleton<IAuditActionFactory<TAction, TRow, TEnum>, TActionFactory>();
        services.AddSingleton<IAuditRowFactory<TRow, TEnum>, TRowFactory>();
        var manager = new AuditSchemeManager<TEnum, TColor>();
        services.AddSingleton<IAuditSchemeManager<TEnum, TColor>, AuditSchemeManager<TEnum, TColor>>((x) => manager);
        services.AddScoped<IAuditPropertyManager, AuditPropertyManager>();
        services.AddScoped<IPostponedAuditManager<TAction, TRow, TEnum, TColor>, PostponedAuditManager<TDbContext, TAction, TRow, TEnum, TColor>>();
        return services;
    }
}
