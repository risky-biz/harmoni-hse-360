using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Harmoni360.Web.Hubs;

[Authorize]
public class IncidentHub : Hub
{
    private readonly ILogger<IncidentHub> _logger;

    public IncidentHub(ILogger<IncidentHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var roles = Context.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();

        // Add user to role-based groups
        foreach (var role in roles)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role-{role}");
        }

        // Add to user-specific group
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        await base.OnConnectedAsync();
        _logger.LogInformation("User {UserId} connected to IncidentHub", userId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} disconnected from IncidentHub", userId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinLocationGroup(string location)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"location-{location}");
    }

    public async Task LeaveLocationGroup(string location)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"location-{location}");
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {UserId} joined group: {GroupName}", Context.UserIdentifier, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("User {UserId} left group: {GroupName}", Context.UserIdentifier, groupName);
    }
}