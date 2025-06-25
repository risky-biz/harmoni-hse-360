using MediatR;

namespace Harmoni360.Application.Features.UserManagement.Queries;

public record GetUserStatisticsQuery : IRequest<UserStatisticsDto>
{
}

public class UserStatisticsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int SuspendedUsers { get; set; }
    public int LockedAccounts { get; set; }
    public int UsersWithMFA { get; set; }
    public int NewUsersThisMonth { get; set; }
    public Dictionary<string, int> UsersByDepartment { get; set; } = new();
    public Dictionary<string, int> UsersByLocation { get; set; } = new();
    public Dictionary<string, int> UsersByRole { get; set; } = new();
}