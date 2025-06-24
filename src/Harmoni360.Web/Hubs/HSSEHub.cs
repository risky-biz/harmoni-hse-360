using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Harmoni360.Web.Hubs;

[Authorize]
public class HSSEHub : Hub
{
    public async Task JoinHSSEDashboardGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "HSSEDashboard");
    }

    public async Task LeaveHSSEDashboardGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "HSSEDashboard");
    }

    public async Task JoinDepartmentGroup(string department)
    {
        if (!string.IsNullOrEmpty(department))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Department_{department}");
        }
    }

    public async Task LeaveDepartmentGroup(string department)
    {
        if (!string.IsNullOrEmpty(department))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Department_{department}");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up any group memberships
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "HSSEDashboard");
        await base.OnDisconnectedAsync(exception);
    }
}