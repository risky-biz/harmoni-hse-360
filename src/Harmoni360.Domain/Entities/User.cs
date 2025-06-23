using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities;

public class User : BaseEntity, IAuditableEntity
{
    // Core fields (maintained for backward compatibility)
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string EmployeeId { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public string Position { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } // Maintained for backward compatibility
    
    // HSSE-specific fields
    public string? PhoneNumber { get; private set; }
    public string? EmergencyContactName { get; private set; }
    public string? EmergencyContactPhone { get; private set; }
    public string? SupervisorEmployeeId { get; private set; }
    public DateTime? HireDate { get; private set; }
    public string? WorkLocation { get; private set; }
    public string? CostCenter { get; private set; }
    
    // Security fields
    public bool RequiresMFA { get; private set; }
    public DateTime? LastPasswordChange { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? AccountLockedUntil { get; private set; }
    
    // User preferences
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

    // Backward compatible Create method
    public static User Create(
        string email,
        string passwordHash,
        string name,
        string employeeId,
        string department,
        string position)
    {
        return Create(email, passwordHash, name, employeeId, department, position, null, null, null, null);
    }
    
    // Enhanced Create method with HSSE fields
    public static User Create(
        string email,
        string passwordHash,
        string name,
        string employeeId,
        string department,
        string position,
        string? phoneNumber,
        string? workLocation,
        string? costCenter,
        DateTime? hireDate)
    {
        var user = new User
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
            IsActive = true,
            Status = UserStatus.Active,
            LastPasswordChange = DateTime.UtcNow,
            RequiresMFA = false,
            FailedLoginAttempts = 0
        };
        return user;
    }

    public void UpdateProfile(string name, string department, string position)
    {
        Name = name;
        Department = department;
        Position = position;
    }
    
    // Enhanced profile update with HSSE fields
    public void UpdateProfile(
        string name, 
        string department, 
        string position,
        string? phoneNumber,
        string? workLocation,
        string? costCenter)
    {
        Name = name;
        Department = department;
        Position = position;
        PhoneNumber = phoneNumber;
        WorkLocation = workLocation;
        CostCenter = costCenter;
    }

    public void Deactivate()
    {
        IsActive = false;
        Status = UserStatus.Inactive;
    }

    public void Activate()
    {
        IsActive = true;
        Status = UserStatus.Active;
    }
    
    // New status management methods
    public void ChangeStatus(UserStatus newStatus)
    {
        Status = newStatus;
        // Maintain backward compatibility with IsActive
        IsActive = (newStatus == UserStatus.Active);
    }
    
    public void Suspend()
    {
        ChangeStatus(UserStatus.Suspended);
    }
    
    public void Terminate()
    {
        ChangeStatus(UserStatus.Terminated);
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        LastPasswordChange = DateTime.UtcNow;
    }
    
    // New HSSE-specific methods
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
    
    public void UpdateHireDate(DateTime? hireDate)
    {
        HireDate = hireDate;
    }
    
    // Security methods
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
    
    public void EnableMFA()
    {
        RequiresMFA = true;
    }
    
    public void DisableMFA()
    {
        RequiresMFA = false;
    }
    
    public void ResetPassword(string newPasswordHash, bool requirePasswordChange = true)
    {
        PasswordHash = newPasswordHash;
        LastPasswordChange = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        AccountLockedUntil = null;
        
        // Force password change on next login if required
        if (requirePasswordChange)
        {
            // This would typically be handled by setting a flag or through authentication system
            // For now, we just ensure the password is updated
        }
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
    
    public bool HasRole(int roleId)
    {
        return _userRoles.Any(ur => ur.RoleId == roleId);
    }
}