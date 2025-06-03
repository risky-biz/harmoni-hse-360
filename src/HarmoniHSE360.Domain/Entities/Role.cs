using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private readonly List<Permission> _permissions = new();
    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

    protected Role() { } // For EF Core

    public static Role Create(string name, string description)
    {
        return new Role
        {
            Name = name,
            Description = description
        };
    }

    public void AddPermission(Permission permission)
    {
        if (!_permissions.Contains(permission))
            _permissions.Add(permission);
    }

    public void RemovePermission(Permission permission)
    {
        _permissions.Remove(permission);
    }
}

public class UserRole : BaseEntity
{
    public int UserId { get; private set; }
    public int RoleId { get; private set; }
    public Role Role { get; private set; } = null!;

    protected UserRole() { } // For EF Core

    public UserRole(int userId, int roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }
}