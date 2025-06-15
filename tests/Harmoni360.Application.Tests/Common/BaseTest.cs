using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Harmoni360.Application.Tests.Common;

public abstract class BaseTest : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly Mock<ICurrentUserService> MockCurrentUserService;

    protected BaseTest()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ApplicationDbContext(options);

        // Mock current user service
        MockCurrentUserService = new Mock<ICurrentUserService>();
        MockCurrentUserService.Setup(x => x.UserId).Returns(1);
        MockCurrentUserService.Setup(x => x.UserName).Returns("test.user@harmoni360.com");
    }

    protected void SeedData()
    {
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

        Context.Users.AddRange(testUser, trainerUser, participantUser);

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

        Context.Departments.Add(testDepartment);
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}