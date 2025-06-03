using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HarmoniHSE360.Web.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    
    public NotificationHub(ILogger<NotificationHub> logger)
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
        _logger.LogInformation("User {UserId} connected to NotificationHub", userId);
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} disconnected from NotificationHub", userId);
        await base.OnDisconnectedAsync(exception);
    }
}