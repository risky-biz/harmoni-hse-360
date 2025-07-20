using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Trainings.Services;
using Harmoni360.Application.Services;
using Harmoni360.Domain.Interfaces;
using Harmoni360.Infrastructure.Persistence;
using Harmoni360.Infrastructure.Persistence.Repositories;
using Harmoni360.Infrastructure.Services;
using Harmoni360.Infrastructure.Services.DataSeeders;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Elsa.Extensions;
using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Harmoni360.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    npgsqlOptions.UseNodaTime();
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                });
        });

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IWasteReportRepository, WasteReportRepository>();

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
            services.AddDistributedMemoryCache();
        }

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddSingleton<IAntivirusScanner, NullAntivirusScanner>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IVideoValidationService, VideoValidationService>();
        
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
        services.AddScoped<WasteDataSeeder>();
        services.AddScoped<ModuleConfigurationDataSeeder>();
        
        services.AddScoped<HSSEHistoricalDataSeeder>();
        services.AddScoped<HSSECrossModuleDataBuilder>();
        services.AddScoped<HSSEKPIBaselineCalculator>();
        
        services.AddScoped<PPEOperationalDataSeeder>();
        services.AddScoped<WasteOperationalDataSeeder>();
        services.AddScoped<SecurityOperationalDataSeeder>();
        services.AddScoped<HealthOperationalDataSeeder>();
        
        services.AddScoped<IIncidentAuditService, IncidentAuditService>();
        services.AddScoped<IHazardAuditService, HazardAuditService>();
        services.AddScoped<IWasteAuditService, WasteAuditService>();
        
        services.AddScoped<ICachedTrainingService, CachedTrainingService>();
        
        services.AddScoped<IHSSECacheService, HSSECacheService>();
        
        services.AddScoped<IMaterializedViewRefreshService, MaterializedViewRefreshService>();
        
        services.AddHostedService<HSSEMaterializedViewBackgroundService>();
        
        services.AddSingleton<IPerformanceMetricsService, PerformanceMetricsService>();

        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
        services.AddScoped<IEscalationService, EscalationService>();

        services.AddScoped<ISecurityIncidentService, SecurityIncidentService>();
        services.AddScoped<ISecurityAuditService, SecurityAuditService>();

        services.AddScoped<IModuleConfigurationService, ModuleConfigurationService>();
        
        services.AddScoped<IModuleDiscoveryService, ModuleDiscoveryService>();

        services.AddSingleton<IApplicationModeService, ApplicationModeService>();

        // Configure Elsa Workflows
        services.AddElsa(elsa => elsa
            .UseWorkflowManagement(management => management.UseEntityFrameworkCore(ef => 
                ef.UsePostgreSql(configuration.GetConnectionString("DefaultConnection")!)))
            .UseWorkflowRuntime(runtime => runtime.UseEntityFrameworkCore(ef => 
                ef.UsePostgreSql(configuration.GetConnectionString("DefaultConnection")!)))
            .UseScheduling()
            .UseJavaScript()
            .UseLiquid()
            .UseCSharp()
            .UseHttp()
            .UseWorkflowsApi()
            // Note: Not using .UseIdentity() to allow integration with existing auth system
        );
        
        // Disable security for Elsa API endpoints
        // This allows Elsa to work with our existing authentication system
        Elsa.EndpointSecurityOptions.DisableSecurity();

        return services;
    }
}