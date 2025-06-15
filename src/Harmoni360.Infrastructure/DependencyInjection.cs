using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Trainings.Services;
using Harmoni360.Domain.Interfaces;
using Harmoni360.Infrastructure.Persistence;
using Harmoni360.Infrastructure.Persistence.Repositories;
using Harmoni360.Infrastructure.Services;
using Harmoni360.Infrastructure.Services.DataSeeders;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Harmoni360.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    npgsqlOptions.UseNodaTime();
                });
        });

        // Register DbContext interface
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Add repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IWasteReportRepository, WasteReportRepository>();

        // Add caching
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "HSE360";
            });
        }
        else
        {
            // Use in-memory cache as fallback
            services.AddMemoryCache();
        }

        // Add infrastructure services
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddSingleton<IAntivirusScanner, NullAntivirusScanner>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        
        // Add data seeders
        services.AddScoped<IDataSeeder, DataSeeder>();
        services.AddScoped<ConfigurationDataSeeder>();
        services.AddScoped<RoleDataSeeder>();
        services.AddScoped<ModulePermissionDataSeeder>();
        services.AddScoped<RoleModulePermissionDataSeeder>();
        services.AddScoped<UserDataSeeder>();
        services.AddScoped<IncidentDataSeeder>();
        services.AddScoped<PPEItemDataSeeder>();
        services.AddScoped<HazardDataSeeder>();
        services.AddScoped<HealthDataSeeder>();
        services.AddScoped<SecurityDataSeeder>();
        services.AddScoped<WorkPermitDataSeeder>();
        services.AddScoped<InspectionDataSeeder>();
        services.AddScoped<AuditDataSeeder>();
        services.AddScoped<TrainingDataSeeder>();
        services.AddScoped<LicenseDataSeeder>();
        
        services.AddScoped<IIncidentAuditService, IncidentAuditService>();
        services.AddScoped<IHazardAuditService, HazardAuditService>();
        
        // Add training services
        services.AddScoped<ICachedTrainingService, CachedTrainingService>();
        
        // Add performance monitoring
        services.AddSingleton<IPerformanceMetricsService, PerformanceMetricsService>();

        // Add notification and escalation services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
        services.AddScoped<IEscalationService, EscalationService>();

        // Add security services
        services.AddScoped<ISecurityIncidentService, SecurityIncidentService>();
        services.AddScoped<ISecurityAuditService, SecurityAuditService>();

        // Add application mode service
        services.AddSingleton<IApplicationModeService, ApplicationModeService>();

        return services;
    }
}