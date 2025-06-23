using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.UserManagement.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public UserStatus Status { get; set; }
    
    // HSSE-specific fields
    public string? PhoneNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? SupervisorEmployeeId { get; set; }
    public DateTime? HireDate { get; set; }
    public string? WorkLocation { get; set; }
    public string? CostCenter { get; set; }
    
    // Security fields
    public bool RequiresMFA { get; set; }
    public DateTime? LastPasswordChange { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? AccountLockedUntil { get; set; }
    
    // User preferences
    public string? PreferredLanguage { get; set; }
    public string? TimeZone { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    
    public List<UserRoleDto> Roles { get; set; } = new();
}

public class UserRoleDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public RoleType RoleType { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

public class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    
    // HSSE-specific fields
    public string? PhoneNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? SupervisorEmployeeId { get; set; }
    public DateTime? HireDate { get; set; }
    public string? WorkLocation { get; set; }
    public string? CostCenter { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? TimeZone { get; set; }
    
    public List<int> RoleIds { get; set; } = new();
}

public class UpdateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public UserStatus Status { get; set; }
    
    // HSSE-specific fields
    public string? PhoneNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? SupervisorEmployeeId { get; set; }
    public DateTime? HireDate { get; set; }
    public string? WorkLocation { get; set; }
    public string? CostCenter { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? TimeZone { get; set; }
    public bool RequiresMFA { get; set; }
    
    public List<int> RoleIds { get; set; } = new();
}

public class UserListDto
{
    public List<UserDto> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RoleType RoleType { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public int UserCount { get; set; }
}