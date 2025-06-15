using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Harmoni360.Integration.Tests.Common;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    public Mock<ICurrentUserService> MockCurrentUserService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the app's ApplicationDbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add ApplicationDbContext using an in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Replace ICurrentUserService with mock
            var currentUserDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ICurrentUserService));
            if (currentUserDescriptor != null)
            {
                services.Remove(currentUserDescriptor);
            }
            services.AddSingleton(MockCurrentUserService.Object);

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

            // Ensure the database is created
            db.Database.EnsureCreated();

            try
            {
                // Seed the database with test data
                SeedTestData(db);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the database with test messages. Error: {Message}", ex.Message);
            }
        });

        builder.UseEnvironment("Testing");
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Clear existing data
        context.Trainings.RemoveRange(context.Trainings);
        context.Users.RemoveRange(context.Users);
        context.Departments.RemoveRange(context.Departments);
        context.SaveChanges();

        // Add test users
        var testUser = new Harmoni360.Domain.Entities.User
        {
            Id = 1,
            Name = "Test User",
            Email = "test.user@harmoni360.com",
            Position = "Safety Manager",
            PasswordHash = "hashedpassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var trainerUser = new Harmoni360.Domain.Entities.User
        {
            Id = 2,
            Name = "John Trainer",
            Email = "john.trainer@harmoni360.com",
            Position = "Senior Safety Trainer",
            PasswordHash = "hashedpassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var participantUser = new Harmoni360.Domain.Entities.User
        {
            Id = 3,
            Name = "Jane Participant",
            Email = "jane.participant@harmoni360.com",
            Position = "Safety Officer",
            PasswordHash = "hashedpassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        context.Users.AddRange(testUser, trainerUser, participantUser);

        // Add test department
        var testDepartment = new Harmoni360.Domain.Entities.Department
        {
            Id = 1,
            Name = "Safety Department",
            Code = "SAFETY",
            Description = "Responsible for workplace safety",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        context.Departments.Add(testDepartment);

        // Add test roles and permissions for authorization testing
        var adminRole = new Harmoni360.Domain.Entities.Role
        {
            Id = 1,
            Name = "Administrator",
            Description = "System Administrator",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var trainerRole = new Harmoni360.Domain.Entities.Role
        {
            Id = 2,
            Name = "Trainer",
            Description = "Training Manager",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        context.Roles.AddRange(adminRole, trainerRole);

        // Add training management permissions
        var trainingModule = new Harmoni360.Domain.Entities.ModulePermission
        {
            Id = 17,
            ModuleType = Harmoni360.Domain.Enums.ModuleType.TrainingManagement,
            PermissionType = Harmoni360.Domain.Enums.PermissionType.Create,
            Description = "Create training sessions"
        };

        context.ModulePermissions.Add(trainingModule);

        // Assign permissions to roles
        var rolePermission = new Harmoni360.Domain.Entities.RoleModulePermission
        {
            RoleId = 1,
            ModulePermissionId = 17
        };

        context.RoleModulePermissions.Add(rolePermission);

        context.SaveChanges();
    }

    public void SetCurrentUser(int userId, string userName)
    {
        MockCurrentUserService.Setup(x => x.UserId).Returns(userId);
        MockCurrentUserService.Setup(x => x.UserName).Returns(userName);
    }
}