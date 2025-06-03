using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Domain.Entities;

public class Permission : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;

    private readonly List<Role> _roles = new();
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    protected Permission() { } // For EF Core

    public static Permission Create(string name, string displayName, string description, string category)
    {
        return new Permission
        {
            Name = name,
            DisplayName = displayName,
            Description = description,
            Category = category
        };
    }

    public void UpdateDetails(string displayName, string description, string category)
    {
        DisplayName = displayName;
        Description = description;
        Category = category;
    }
}