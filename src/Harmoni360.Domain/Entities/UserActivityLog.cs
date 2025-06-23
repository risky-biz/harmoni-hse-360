using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities;

public class UserActivityLog : BaseEntity
{
    public int UserId { get; private set; }
    public string ActivityType { get; private set; } = string.Empty;
    public string? EntityType { get; private set; }
    public int? EntityId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    protected UserActivityLog() { } // For EF Core

    public static UserActivityLog Create(
        int userId,
        string activityType,
        string description,
        string? entityType = null,
        int? entityId = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new UserActivityLog
        {
            UserId = userId,
            ActivityType = activityType,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };
    }
}