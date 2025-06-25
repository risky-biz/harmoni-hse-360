using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.UserManagement.DTOs;

public class UserSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Department { get; set; }
    public string? WorkLocation { get; set; }
    public UserStatus? Status { get; set; }
    public int? RoleId { get; set; }
    public bool? RequiresMFA { get; set; }
    public bool? IsLocked { get; set; }
    public DateTime? HiredAfter { get; set; }
    public DateTime? HiredBefore { get; set; }
    public string? SupervisorEmployeeId { get; set; }
    
    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
}