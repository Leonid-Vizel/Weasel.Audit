using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weasel.Audit.Services;

namespace Weasel.Audit.DependencyInjection.Extensions;

public static class AuditServiceCollectionExtensions
{
    public static IServiceCollection AddResponseCompression<TDbContext, TFactoryImplementation>(this IServiceCollection services)
        where TFactoryImplementation : class, IAuditActionFactory
        where TDbContext : DbContext
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        services.TryAddSingleton<IAuditStateStorage, AuditStateStorage>();
        services.TryAddSingleton<IAuditPropertyStorage, AuditPropertyStorage>();
        services.TryAddSingleton<IAuditActionFactory, TFactoryImplementation>();
        services.TryAddSingleton<IAuditSchemeManager, AuditSchemeManager>();
        services.TryAddScoped<IAuditPropertyManager, AuditPropertyManager>();
        services.TryAddScoped<IPostponedAuditManager, PostponedAuditManager<TDbContext>>();
        return services;
    }
}
