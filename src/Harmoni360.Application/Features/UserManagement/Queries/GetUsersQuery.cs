using MediatR;
using Harmoni360.Application.Features.UserManagement.DTOs;

namespace Harmoni360.Application.Features.UserManagement.Queries;

public record GetUsersQuery : IRequest<UserListDto>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public string? Department { get; init; }
    public bool? IsActive { get; init; }
    public int? RoleId { get; init; }
    public string? SortBy { get; init; } = "name";
    public bool SortDescending { get; init; } = false;
}