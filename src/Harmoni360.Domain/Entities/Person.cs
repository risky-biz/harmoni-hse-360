using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public class Person : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string? Department { get; private set; }
    public string? Position { get; private set; }
    public PersonType PersonType { get; private set; }
    public string? EmployeeId { get; private set; }
    public bool IsActive { get; private set; }
    
    // Linked User (nullable - person doesn't have to be a system user)
    public int? UserId { get; private set; }
    public User? User { get; private set; }
    
    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    // Navigation properties
    private readonly List<HealthRecord> _healthRecords = new();
    public IReadOnlyCollection<HealthRecord> HealthRecords => _healthRecords.AsReadOnly();

    protected Person() { } // For EF Core

    public static Person Create(
        string name,
        string email,
        PersonType personType,
        string? phoneNumber = null,
        string? department = null,
        string? position = null,
        string? employeeId = null,
        int? userId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        var person = new Person
        {
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PersonType = personType,
            PhoneNumber = phoneNumber?.Trim(),
            Department = department?.Trim(),
            Position = position?.Trim(),
            EmployeeId = employeeId?.Trim(),
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System" // Will be set by infrastructure
        };

        return person;
    }

    public void UpdateInfo(
        string name,
        string email,
        string? phoneNumber = null,
        string? department = null,
        string? position = null,
        string? employeeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
        PhoneNumber = phoneNumber?.Trim();
        Department = department?.Trim();
        Position = position?.Trim();
        EmployeeId = employeeId?.Trim();
        LastModifiedAt = DateTime.UtcNow;
    }

    public void LinkUser(int userId)
    {
        UserId = userId;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UnlinkUser()
    {
        UserId = null;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastModifiedAt = DateTime.UtcNow;
    }
}