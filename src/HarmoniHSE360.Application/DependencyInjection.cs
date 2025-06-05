using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;
using HarmoniHSE360.Application.Common.Behaviors;
using HarmoniHSE360.Application.Common.Interfaces;

namespace HarmoniHSE360.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Add AutoMapper
        services.AddAutoMapper(assembly);

        // Add FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Add Pipeline Behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        // Add module-specific services
        services.AddIncidentModule();

        return services;
    }

    private static IServiceCollection AddIncidentModule(this IServiceCollection services)
    {
        // Register cache services
        services.AddSingleton<ICacheService, CacheService>();
        services.AddScoped<IIncidentCacheService, IncidentCacheService>();

        // Register incident-specific services here
        return services;
    }
}