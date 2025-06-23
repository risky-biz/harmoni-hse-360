using MediatR;
using Harmoni360.Application.Features.UserManagement.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.UserManagement.Queries;

public record GetUsersQuery : IRequest<UserListDto>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public string? Department { get; init; }
    public string? WorkLocation { get; init; }
    public bool? IsActive { get; init; }
    public UserStatus? Status { get; init; }
    public int? RoleId { get; init; }
    public bool? RequiresMFA { get; init; }
    public bool? IsLocked { get; init; }
    public DateTime? HiredAfter { get; init; }
    public DateTime? HiredBefore { get; init; }
    public string? SupervisorEmployeeId { get; init; }
    public string? SortBy { get; init; } = "name";
    public bool SortDescending { get; init; } = false;
}