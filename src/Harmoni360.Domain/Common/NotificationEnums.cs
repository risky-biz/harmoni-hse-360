namespace Harmoni360.Domain.Common;

public enum NotificationChannel
{
    Email = 1,
    Sms = 2,
    WhatsApp = 3,
    Push = 4,
    InApp = 5
}

public enum NotificationPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4,
    Emergency = 5
}