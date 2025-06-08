using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Infrastructure.Persistence;
using Harmoni360.Infrastructure;

namespace DatabaseMigrator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Harmoni360 Database Migrator ===");
            
            var host = CreateHostBuilder(args).Build();
            
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();
            
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                
                logger.LogInformation("Checking database connection...");
                var canConnect = await context.Database.CanConnectAsync();
                logger.LogInformation($"Database connection: {(canConnect ? "SUCCESS" : "FAILED")}");
                
                if (!canConnect)
                {
                    logger.LogError("Cannot connect to database. Check connection string.");
                    return;
                }
                
                // Check current migration status
                logger.LogInformation("Checking migration status...");
                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                
                logger.LogInformation($"Applied migrations: {appliedMigrations.Count()}");
                foreach (var migration in appliedMigrations)
                {
                    logger.LogInformation($"  ✓ {migration}");
                }
                
                logger.LogInformation($"Pending migrations: {pendingMigrations.Count()}");
                foreach (var migration in pendingMigrations)
                {
                    logger.LogInformation($"  ⏳ {migration}");
                }
                
                // Check if ModulePermissions table exists
                logger.LogInformation("Checking if ModulePermissions table exists...");
                try
                {
                    var modulePermissionCount = await context.ModulePermissions.CountAsync();
                    logger.LogInformation($"ModulePermissions table exists with {modulePermissionCount} records");
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"ModulePermissions table does not exist: {ex.Message}");
                    
                    // Reset migration to force reapplication
                    logger.LogInformation("Resetting migration history for 20250606143400_AddModuleBasedAuthorization...");
                    await context.Database.ExecuteSqlRawAsync(@"
                        DELETE FROM ""__EFMigrationsHistory"" 
                        WHERE ""MigrationId"" = '20250606143400_AddModuleBasedAuthorization'
                    ");
                    
                    logger.LogInformation("Migration history reset. Checking for pending migrations again...");
                    var newPendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    logger.LogInformation($"New pending migrations: {newPendingMigrations.Count()}");
                    
                    if (newPendingMigrations.Any())
                    {
                        logger.LogInformation("Applying pending migrations...");
                        await context.Database.MigrateAsync();
                        logger.LogInformation("✅ Migrations applied successfully!");
                        
                        // Verify tables were created
                        var modulePermissionCountAfter = await context.ModulePermissions.CountAsync();
                        logger.LogInformation($"ModulePermissions table now has {modulePermissionCountAfter} records");
                    }
                }
                
                // Check Role table structure
                logger.LogInformation("Checking Role table structure...");
                var roleCount = await context.Roles.CountAsync();
                logger.LogInformation($"Roles table has {roleCount} records");
                
                // Try to check if RoleType column exists
                try
                {
                    var rolesWithRoleType = await context.Roles
                        .Where(r => r.RoleType != Harmoni360.Domain.Enums.RoleType.Viewer)
                        .CountAsync();
                    logger.LogInformation($"Roles with non-default RoleType: {rolesWithRoleType}");
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"RoleType column issue: {ex.Message}");
                }
                
                logger.LogInformation("✅ Database migration check completed successfully!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ An error occurred during migration");
                throw;
            }
        }
        
        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false);
                    config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Add infrastructure services
                    services.AddInfrastructure(context.Configuration);
                    
                    // Override some services that might cause issues
                    services.AddLogging(builder => 
                    {
                        builder.ClearProviders();
                        builder.AddConsole();
                    });
                });
    }
}