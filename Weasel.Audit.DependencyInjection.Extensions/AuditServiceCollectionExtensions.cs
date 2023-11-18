using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Weasel.Audit.Interfaces;
using Weasel.Audit.Services;

namespace Weasel.Audit.DependencyInjection.Extensions;

public static class AuditServiceCollectionExtensions
{
    public static IServiceCollection AddAudit<TDbContext, TFactoryImplementation, TAuditAction>(this IServiceCollection services, params Type[] auditEnumTypes)
        where TFactoryImplementation : class, IAuditActionFactory<TAuditAction>
        where TAuditAction : class, IAuditAction
        where TDbContext : DbContext
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        services.AddSingleton<IAuditPropertyStorage, AuditPropertyStorage>();
        services.AddSingleton<IAuditActionFactory<TAuditAction>, TFactoryImplementation>();
        var manager = new AuditSchemeManager(auditEnumTypes);
        services.AddSingleton<IAuditSchemeManager, AuditSchemeManager>((x) => manager);
        services.AddScoped<IAuditPropertyManager, AuditPropertyManager>();
        services.AddScoped<IPostponedAuditManager<TAuditAction>, PostponedAuditManager<TDbContext, TAuditAction>>();
        return services;
    }
}
