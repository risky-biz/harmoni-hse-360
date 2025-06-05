using HarmoniHSE360.Domain.Common;

namespace HarmoniHSE360.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendEmailAsync(EmailNotification notification, CancellationToken cancellationToken = default);
    Task SendSmsAsync(SmsNotification notification, CancellationToken cancellationToken = default);
    Task SendWhatsAppAsync(WhatsAppNotification notification, CancellationToken cancellationToken = default);
    Task SendPushNotificationAsync(PushNotification notification, CancellationToken cancellationToken = default);
    Task SendMultiChannelAsync(MultiChannelNotification notification, CancellationToken cancellationToken = default);
}

public class EmailNotification
{
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public List<EmailAttachment> Attachments { get; set; } = new();
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class SmsNotification
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class WhatsAppNotification
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public Dictionary<string, string> TemplateParameters { get; set; } = new();
    public List<WhatsAppMedia> Media { get; set; } = new();
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class PushNotification
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? ClickAction { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
}

public class MultiChannelNotification
{
    public string UserId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<NotificationChannel> Channels { get; set; } = new();
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public Dictionary<string, string> Metadata { get; set; } = new();
    public TimeSpan? DelayBetweenChannels { get; set; }
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
}

public class WhatsAppMedia
{
    public string Type { get; set; } = string.Empty; // image, video, document
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string? FileName { get; set; }
}

