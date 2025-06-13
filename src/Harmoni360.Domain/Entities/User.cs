using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public class User : BaseEntity, IAuditableEntity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string EmployeeId { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public string Position { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

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
        string position)
    {
        return new User
        {
            Email = email,
            PasswordHash = passwordHash,
            Name = name,
            EmployeeId = employeeId,
            Department = department,
            Position = position,
            IsActive = true
        };
    }

    public void UpdateProfile(string name, string department, string position)
    {
        Name = name;
        Department = department;
        Position = position;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void AssignRole(Role role)
    {
        if (_userRoles.Any(ur => ur.RoleId == role.Id))
            return;

        _userRoles.Add(new UserRole(Id, role.Id));
    }
}