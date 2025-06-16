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
        // Mock current user service
        MockCurrentUserService = new Mock<ICurrentUserService>();
        MockCurrentUserService.Setup(x => x.UserId).Returns(1);
        MockCurrentUserService.Setup(x => x.Email).Returns("test.user@harmoni360.com");
        MockCurrentUserService.Setup(x => x.Name).Returns("Test User");
        MockCurrentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        MockCurrentUserService.Setup(x => x.Roles).Returns(new List<string> { "User" });

        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ApplicationDbContext(options, MockCurrentUserService.Object);
    }

    protected void SeedData()
    {
        // Add test users using factory methods
        var testUser = Harmoni360.Domain.Entities.User.Create(
            "test.user@harmoni360.com",
            "hashedpassword",
            "Test User",
            "EMP001",
            "Safety Department",
            "Safety Manager");

        var trainerUser = Harmoni360.Domain.Entities.User.Create(
            "john.trainer@harmoni360.com",
            "hashedpassword",
            "John Trainer",
            "EMP002",
            "Safety Department",
            "Senior Safety Trainer");

        var participantUser = Harmoni360.Domain.Entities.User.Create(
            "jane.participant@harmoni360.com",
            "hashedpassword",
            "Jane Participant",
            "EMP003",
            "Safety Department",
            "Safety Officer");

        Context.Users.AddRange(testUser, trainerUser, participantUser);

        // Add test department using factory method
        var testDepartment = Harmoni360.Domain.Entities.Department.Create(
            "Safety Department",
            "SAFETY",
            "Responsible for workplace safety",
            isActive: true);

        Context.Departments.Add(testDepartment);
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}