# Technical Implementation Guide
## Enterprise User Management for Harmoni360 HSSE

### Implementation Overview

This guide provides step-by-step technical implementation instructions for the Enterprise User Management system in Harmoni360 HSSE application. The implementation follows CQRS pattern, Clean Architecture principles, and integrates with the existing modular monolith structure.

### Prerequisites

**Technical Requirements:**
- .NET 8 SDK
- Entity Framework Core 8.x
- PostgreSQL 14+
- Redis 6.x (for caching)
- React 18+ with TypeScript
- SignalR for real-time notifications

**Development Environment:**
- Visual Studio 2022 or VS Code
- Node.js 18+
- Docker Desktop (for local development)
- Postman or similar API testing tool

### Phase 1: Database Schema Implementation

#### Step 1.1: Create Migration for User Entity Enhancements

```bash
# Navigate to Infrastructure project
cd src/Harmoni360.Infrastructure

# Create migration
dotnet ef migrations add EnhanceUserEntityForHSSE --startup-project ../Harmoni360.Web
```

**Migration Content:**
```csharp
public partial class EnhanceUserEntityForHSSE : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add new columns to Users table
        migrationBuilder.AddColumn<string>(
            name: "PhoneNumber",
            table: "Users",
            type: "character varying(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "EmergencyContactName",
            table: "Users",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "EmergencyContactPhone",
            table: "Users",
            type: "character varying(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SupervisorEmployeeId",
            table: "Users",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "HireDate",
            table: "Users",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "WorkLocation",
            table: "Users",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "CostCenter",
            table: "Users",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "RequiresMFA",
            table: "Users",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTime>(
            name: "LastPasswordChange",
            table: "Users",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "LastLoginAt",
            table: "Users",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "FailedLoginAttempts",
            table: "Users",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<DateTime>(
            name: "AccountLockedUntil",
            table: "Users",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PreferredLanguage",
            table: "Users",
            type: "character varying(5)",
            maxLength: 5,
            nullable: true,
            defaultValue: "en");

        migrationBuilder.AddColumn<string>(
            name: "TimeZone",
            table: "Users",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "Status",
            table: "Users",
            type: "integer",
            nullable: false,
            defaultValue: 1);

        // Create indexes for performance
        migrationBuilder.CreateIndex(
            name: "IX_Users_Status",
            table: "Users",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Department",
            table: "Users",
            column: "Department");

        migrationBuilder.CreateIndex(
            name: "IX_Users_WorkLocation",
            table: "Users",
            column: "WorkLocation");

        migrationBuilder.CreateIndex(
            name: "IX_Users_SupervisorEmployeeId",
            table: "Users",
            column: "SupervisorEmployeeId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Remove indexes
        migrationBuilder.DropIndex(name: "IX_Users_Status", table: "Users");
        migrationBuilder.DropIndex(name: "IX_Users_Department", table: "Users");
        migrationBuilder.DropIndex(name: "IX_Users_WorkLocation", table: "Users");
        migrationBuilder.DropIndex(name: "IX_Users_SupervisorEmployeeId", table: "Users");

        // Remove columns
        migrationBuilder.DropColumn(name: "PhoneNumber", table: "Users");
        migrationBuilder.DropColumn(name: "EmergencyContactName", table: "Users");
        migrationBuilder.DropColumn(name: "EmergencyContactPhone", table: "Users");
        migrationBuilder.DropColumn(name: "SupervisorEmployeeId", table: "Users");
        migrationBuilder.DropColumn(name: "HireDate", table: "Users");
        migrationBuilder.DropColumn(name: "WorkLocation", table: "Users");
        migrationBuilder.DropColumn(name: "CostCenter", table: "Users");
        migrationBuilder.DropColumn(name: "RequiresMFA", table: "Users");
        migrationBuilder.DropColumn(name: "LastPasswordChange", table: "Users");
        migrationBuilder.DropColumn(name: "LastLoginAt", table: "Users");
        migrationBuilder.DropColumn(name: "FailedLoginAttempts", table: "Users");
        migrationBuilder.DropColumn(name: "AccountLockedUntil", table: "Users");
        migrationBuilder.DropColumn(name: "PreferredLanguage", table: "Users");
        migrationBuilder.DropColumn(name: "TimeZone", table: "Users");
        migrationBuilder.DropColumn(name: "Status", table: "Users");
    }
}
```

#### Step 1.2: Create User Activity Log Table

```bash
dotnet ef migrations add CreateUserActivityLogTable --startup-project ../Harmoni360.Web
```

**Migration Content:**
```csharp
public partial class CreateUserActivityLogTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserActivityLogs",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<int>(type: "integer", nullable: false),
                ActivityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                EntityId = table.Column<int>(type: "integer", nullable: true),
                Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserActivityLogs", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserActivityLogs_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserActivityLogs_UserId_CreatedAt",
            table: "UserActivityLogs",
            columns: new[] { "UserId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_UserActivityLogs_ActivityType",
            table: "UserActivityLogs",
            column: "ActivityType");

        migrationBuilder.CreateIndex(
            name: "IX_UserActivityLogs_CreatedAt",
            table: "UserActivityLogs",
            column: "CreatedAt");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "UserActivityLogs");
    }
}
```

#### Step 1.3: Apply Migrations

```bash
# Apply migrations to database
dotnet ef database update --startup-project ../Harmoni360.Web
```

### Phase 2: Domain Layer Implementation

#### Step 2.1: Update User Entity

**File: `src/Harmoni360.Domain/Entities/User.cs`**

```csharp
using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class User : BaseEntity, IAuditableEntity
{
    // Existing properties...
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string EmployeeId { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public string Position { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    // New HSSE-specific properties
    public string? PhoneNumber { get; private set; }
    public string? EmergencyContactName { get; private set; }
    public string? EmergencyContactPhone { get; private set; }
    public string? SupervisorEmployeeId { get; private set; }
    public DateTime? HireDate { get; private set; }
    public string? WorkLocation { get; private set; }
    public string? CostCenter { get; private set; }
    public bool RequiresMFA { get; private set; }
    public DateTime? LastPasswordChange { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? AccountLockedUntil { get; private set; }
    public string? PreferredLanguage { get; private set; } = "en";
    public string? TimeZone { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.Active;

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    // Audit properties
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected User() { } // For EF Core

    public static User Create(
        string email,
        string passwordHash,
        string name,
        string employeeId,
        string department,
        string position,
        string? phoneNumber = null,
        string? workLocation = null,
        string? costCenter = null,
        DateTime? hireDate = null)
    {
        return new User
        {
            Email = email,
            PasswordHash = passwordHash,
            Name = name,
            EmployeeId = employeeId,
            Department = department,
            Position = position,
            PhoneNumber = phoneNumber,
            WorkLocation = workLocation,
            CostCenter = costCenter,
            HireDate = hireDate,
            Status = UserStatus.Active,
            LastPasswordChange = DateTime.UtcNow
        };
    }

    public void UpdateProfile(
        string name, 
        string department, 
        string position,
        string? phoneNumber = null,
        string? workLocation = null,
        string? costCenter = null)
    {
        Name = name;
        Department = department;
        Position = position;
        PhoneNumber = phoneNumber;
        WorkLocation = workLocation;
        CostCenter = costCenter;
    }

    public void UpdateEmergencyContact(string? contactName, string? contactPhone)
    {
        EmergencyContactName = contactName;
        EmergencyContactPhone = contactPhone;
    }

    public void UpdateSupervisor(string? supervisorEmployeeId)
    {
        SupervisorEmployeeId = supervisorEmployeeId;
    }

    public void UpdatePreferences(string? preferredLanguage, string? timeZone)
    {
        PreferredLanguage = preferredLanguage ?? "en";
        TimeZone = timeZone;
    }

    public void ChangeStatus(UserStatus newStatus)
    {
        Status = newStatus;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        AccountLockedUntil = null;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        
        // Lock account after 5 failed attempts for 30 minutes
        if (FailedLoginAttempts >= 5)
        {
            AccountLockedUntil = DateTime.UtcNow.AddMinutes(30);
        }
    }

    public void UnlockAccount()
    {
        FailedLoginAttempts = 0;
        AccountLockedUntil = null;
    }

    public bool IsAccountLocked()
    {
        return AccountLockedUntil.HasValue && AccountLockedUntil.Value > DateTime.UtcNow;
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        LastPasswordChange = DateTime.UtcNow;
    }

    public void AssignRole(Role role)
    {
        if (_userRoles.Any(ur => ur.RoleId == role.Id))
            return;

        _userRoles.Add(new UserRole(Id, role.Id));
    }

    public void RemoveRole(int roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole != null)
        {
            _userRoles.Remove(userRole);
        }
    }
}
```

#### Step 2.2: Create UserStatus Enum

**File: `src/Harmoni360.Domain/Enums/UserStatus.cs`**

```csharp
namespace Harmoni360.Domain.Enums;

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3,
    PendingActivation = 4,
    Terminated = 5
}
```

#### Step 2.3: Create UserActivityLog Entity

**File: `src/Harmoni360.Domain/Entities/UserActivityLog.cs`**

```csharp
using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public class UserActivityLog : BaseEntity
{
    public int UserId { get; private set; }
    public string ActivityType { get; private set; } = string.Empty;
    public string? EntityType { get; private set; }
    public int? EntityId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    protected UserActivityLog() { } // For EF Core

    public static UserActivityLog Create(
        int userId,
        string activityType,
        string description,
        string? entityType = null,
        int? entityId = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new UserActivityLog
        {
            UserId = userId,
            ActivityType = activityType,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

### Phase 3: Infrastructure Layer Implementation

#### Step 3.1: Update User Configuration

**File: `src/Harmoni360.Infrastructure/Persistence/Configurations/UserConfiguration.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        // Existing configurations
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.EmployeeId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Department)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Position)
            .IsRequired()
            .HasMaxLength(100);

        // New HSSE-specific configurations
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.EmergencyContactName)
            .HasMaxLength(100);

        builder.Property(u => u.EmergencyContactPhone)
            .HasMaxLength(20);

        builder.Property(u => u.SupervisorEmployeeId)
            .HasMaxLength(50);

        builder.Property(u => u.WorkLocation)
            .HasMaxLength(100);

        builder.Property(u => u.CostCenter)
            .HasMaxLength(50);

        builder.Property(u => u.PreferredLanguage)
            .HasMaxLength(5)
            .HasDefaultValue("en");

        builder.Property(u => u.TimeZone)
            .HasMaxLength(50);

        builder.Property(u => u.Status)
            .HasConversion<int>()
            .HasDefaultValue(UserStatus.Active);

        builder.Property(u => u.RequiresMFA)
            .HasDefaultValue(false);

        builder.Property(u => u.FailedLoginAttempts)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.EmployeeId)
            .IsUnique();

        builder.HasIndex(u => u.Status);
        builder.HasIndex(u => u.Department);
        builder.HasIndex(u => u.WorkLocation);
        builder.HasIndex(u => u.SupervisorEmployeeId);

        // Configure navigation properties
        builder.HasMany(u => u.UserRoles)
            .WithOne()
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### Step 3.2: Create UserActivityLog Configuration

**File: `src/Harmoni360.Infrastructure/Persistence/Configurations/UserActivityLogConfiguration.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class UserActivityLogConfiguration : IEntityTypeConfiguration<UserActivityLog>
{
    public void Configure(EntityTypeBuilder<UserActivityLog> builder)
    {
        builder.HasKey(ual => ual.Id);

        builder.Property(ual => ual.ActivityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ual => ual.EntityType)
            .HasMaxLength(50);

        builder.Property(ual => ual.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ual => ual.IpAddress)
            .HasMaxLength(45);

        builder.Property(ual => ual.UserAgent)
            .HasMaxLength(500);

        builder.Property(ual => ual.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes for performance
        builder.HasIndex(ual => new { ual.UserId, ual.CreatedAt });
        builder.HasIndex(ual => ual.ActivityType);
        builder.HasIndex(ual => ual.CreatedAt);

        // Foreign key relationship
        builder.HasOne(ual => ual.User)
            .WithMany()
            .HasForeignKey(ual => ual.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### Step 3.3: Update ApplicationDbContext

**File: `src/Harmoni360.Infrastructure/Persistence/ApplicationDbContext.cs`**

Add the new DbSet:

```csharp
public DbSet<UserActivityLog> UserActivityLogs { get; set; }
```

And in the OnModelCreating method:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Existing configurations...

    modelBuilder.ApplyConfiguration(new UserActivityLogConfiguration());

    base.OnModelCreating(modelBuilder);
}
```
