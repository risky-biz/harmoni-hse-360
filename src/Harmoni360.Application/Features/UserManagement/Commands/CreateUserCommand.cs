using MediatR;
using Harmoni360.Application.Features.UserManagement.DTOs;

namespace Harmoni360.Application.Features.UserManagement.Commands;

public record CreateUserCommand : IRequest<UserDto>
{
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string EmployeeId { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    
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
    
    public List<int> RoleIds { get; init; } = new();
}