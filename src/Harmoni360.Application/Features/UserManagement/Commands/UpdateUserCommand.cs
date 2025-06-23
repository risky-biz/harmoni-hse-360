using MediatR;
using Harmoni360.Application.Features.UserManagement.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public record UpdateUserCommand : IRequest<UserDto>
{
    public int UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public UserStatus Status { get; init; }
    
    // HSSE-specific fields
    public string? PhoneNumber { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? SupervisorEmployeeId { get; init; }
    public DateTime? HireDate { get; init; }
    public string? WorkLocation { get; init; }
    public string? CostCenter { get; init; }
    public string? PreferredLanguage { get; init; }
    public string? TimeZone { get; init; }
    public bool RequiresMFA { get; init; }
    
    public List<int> RoleIds { get; init; } = new();
}