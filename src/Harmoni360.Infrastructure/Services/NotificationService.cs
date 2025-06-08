using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Common;
// using Harmoni360.Web.Hubs; // TODO: Fix circular dependency
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;
using System.Text.Json;

namespace Harmoni360.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly SmtpClient _smtpClient;

    public NotificationService(
        ILogger<NotificationService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _smtpClient = ConfigureSmtpClient();
    }

    public async Task SendEmailAsync(EmailNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending email notification to {To} with subject {Subject}",
                notification.To, notification.Subject);

            var message = new MailMessage
            {
                From = new MailAddress(_configuration["Email:FromAddress"] ?? "noreply@harmoni360.com",
                                     _configuration["Email:FromName"] ?? "Harmoni360"),
                Subject = notification.Subject,
                Body = notification.HtmlBody ?? notification.Body,
                IsBodyHtml = !string.IsNullOrEmpty(notification.HtmlBody)
            };

            message.To.Add(notification.To);

            if (!string.IsNullOrEmpty(notification.Cc))
                message.CC.Add(notification.Cc);

            if (!string.IsNullOrEmpty(notification.Bcc))
                message.Bcc.Add(notification.Bcc);

            // Add attachments
            foreach (var attachment in notification.Attachments)
            {
                var stream = new MemoryStream(attachment.Content);
                message.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
            }

            // Add priority
            message.Priority = notification.Priority switch
            {
                NotificationPriority.Critical or NotificationPriority.Emergency => MailPriority.High,
                NotificationPriority.Low => MailPriority.Low,
                _ => MailPriority.Normal
            };

            await _smtpClient.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("Email notification sent successfully to {To}", notification.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification to {To}", notification.To);
            throw;
        }
    }

    public async Task SendSmsAsync(SmsNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending SMS notification to {PhoneNumber}", notification.PhoneNumber);

            // In a real implementation, you would integrate with SMS providers like:
            // - Twilio
            // - AWS SNS
            // - Azure Communication Services
            // - Local Indonesian SMS gateways

            var smsGatewayUrl = _configuration["Sms:GatewayUrl"];
            var apiKey = _configuration["Sms:ApiKey"];

            if (string.IsNullOrEmpty(smsGatewayUrl))
            {
                _logger.LogWarning("SMS gateway not configured. SMS notification to {PhoneNumber} will be logged only",
                    notification.PhoneNumber);

                // Log the SMS for development/testing
                _logger.LogInformation("SMS Content: To={PhoneNumber}, Message={Message}",
                    notification.PhoneNumber, notification.Message);
                return;
            }

            using var httpClient = new HttpClient();
            var smsPayload = new
            {
                to = notification.PhoneNumber,
                message = notification.Message,
                priority = notification.Priority.ToString()
            };

            var content = new StringContent(JsonSerializer.Serialize(smsPayload),
                System.Text.Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            var response = await httpClient.PostAsync(smsGatewayUrl, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("SMS notification sent successfully to {PhoneNumber}", notification.PhoneNumber);
            }
            else
            {
                _logger.LogError("Failed to send SMS notification to {PhoneNumber}. Status: {StatusCode}",
                    notification.PhoneNumber, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS notification to {PhoneNumber}", notification.PhoneNumber);
            throw;
        }
    }

    public async Task SendWhatsAppAsync(WhatsAppNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending WhatsApp notification to {PhoneNumber}", notification.PhoneNumber);

            // WhatsApp Business API integration
            var whatsappApiUrl = _configuration["WhatsApp:ApiUrl"];
            var accessToken = _configuration["WhatsApp:AccessToken"];

            if (string.IsNullOrEmpty(whatsappApiUrl))
            {
                _logger.LogWarning("WhatsApp API not configured. Notification to {PhoneNumber} will be logged only",
                    notification.PhoneNumber);

                // Log the WhatsApp message for development/testing
                _logger.LogInformation("WhatsApp Content: To={PhoneNumber}, Message={Message}",
                    notification.PhoneNumber, notification.Message);
                return;
            }

            using var httpClient = new HttpClient();

            object whatsappPayload;

            if (!string.IsNullOrEmpty(notification.TemplateId))
            {
                // Use template message
                whatsappPayload = new
                {
                    messaging_product = "whatsapp",
                    to = notification.PhoneNumber,
                    type = "template",
                    template = new
                    {
                        name = notification.TemplateId,
                        language = new { code = "en" },
                        components = notification.TemplateParameters.Select(kvp => new
                        {
                            type = "body",
                            parameters = new[] { new { type = "text", text = kvp.Value } }
                        }).ToArray()
                    }
                };
            }
            else
            {
                // Use text message
                whatsappPayload = new
                {
                    messaging_product = "whatsapp",
                    to = notification.PhoneNumber,
                    type = "text",
                    text = new { body = notification.Message }
                };
            }

            var content = new StringContent(JsonSerializer.Serialize(whatsappPayload),
                System.Text.Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            var response = await httpClient.PostAsync($"{whatsappApiUrl}/messages", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("WhatsApp notification sent successfully to {PhoneNumber}", notification.PhoneNumber);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send WhatsApp notification to {PhoneNumber}. Status: {StatusCode}, Error: {Error}",
                    notification.PhoneNumber, response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp notification to {PhoneNumber}", notification.PhoneNumber);
            throw;
        }
    }

    public Task SendPushNotificationAsync(PushNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending push notification to user {UserId}", notification.UserId);

            // In a real implementation, you would integrate with push notification services like:
            // - Firebase Cloud Messaging (FCM)
            // - Apple Push Notification Service (APNs)
            // - Azure Notification Hubs
            // - OneSignal

            var fcmServerKey = _configuration["Push:FcmServerKey"];

            if (string.IsNullOrEmpty(fcmServerKey))
            {
                _logger.LogWarning("Push notification service not configured. Notification to {UserId} will be logged only",
                    notification.UserId);

                // Log the push notification for development/testing
                _logger.LogInformation("Push Notification: UserId={UserId}, Title={Title}, Body={Body}",
                    notification.UserId, notification.Title, notification.Body);
                return Task.CompletedTask;
            }

            // In a real implementation, you would:
            // 1. Look up user's device tokens from the database
            // 2. Send to FCM/APNs with the appropriate payload
            // 3. Handle token refresh and invalid tokens

            _logger.LogInformation("Push notification prepared for user {UserId}", notification.UserId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to user {UserId}", notification.UserId);
            throw;
        }
    }

    public async Task SendMultiChannelAsync(MultiChannelNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending multi-channel notification to user {UserId} via {ChannelCount} channels",
                notification.UserId, notification.Channels.Count);

            var tasks = new List<Task>();

            foreach (var channel in notification.Channels)
            {
                Task channelTask = channel switch
                {
                    NotificationChannel.Email => SendEmailForUser(notification, cancellationToken),
                    NotificationChannel.Sms => SendSmsForUser(notification, cancellationToken),
                    NotificationChannel.WhatsApp => SendWhatsAppForUser(notification, cancellationToken),
                    NotificationChannel.Push => SendPushForUser(notification, cancellationToken),
                    _ => Task.CompletedTask
                };

                if (notification.DelayBetweenChannels.HasValue)
                {
                    tasks.Add(Task.Delay(notification.DelayBetweenChannels.Value, cancellationToken)
                        .ContinueWith(_ => channelTask, cancellationToken).Unwrap());
                }
                else
                {
                    tasks.Add(channelTask);
                }
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Multi-channel notification completed for user {UserId}", notification.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send multi-channel notification to user {UserId}", notification.UserId);
            throw;
        }
    }

    private SmtpClient ConfigureSmtpClient()
    {
        var smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];
        var enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");

        var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = enableSsl,
            UseDefaultCredentials = string.IsNullOrEmpty(username)
        };

        if (!string.IsNullOrEmpty(username))
        {
            client.Credentials = new NetworkCredential(username, password);
        }

        return client;
    }

    private async Task SendEmailForUser(MultiChannelNotification notification, CancellationToken cancellationToken)
    {
        // In a real implementation, look up user's email from database
        var userEmail = await GetUserEmailAsync(notification.UserId, cancellationToken);

        if (!string.IsNullOrEmpty(userEmail))
        {
            var emailNotification = new EmailNotification
            {
                To = userEmail,
                Subject = notification.Subject,
                Body = notification.Message,
                Priority = notification.Priority,
                Metadata = notification.Metadata
            };

            await SendEmailAsync(emailNotification, cancellationToken);
        }
    }

    private async Task SendSmsForUser(MultiChannelNotification notification, CancellationToken cancellationToken)
    {
        // In a real implementation, look up user's phone from database
        var userPhone = await GetUserPhoneAsync(notification.UserId, cancellationToken);

        if (!string.IsNullOrEmpty(userPhone))
        {
            var smsNotification = new SmsNotification
            {
                PhoneNumber = userPhone,
                Message = notification.Message,
                Priority = notification.Priority,
                Metadata = notification.Metadata
            };

            await SendSmsAsync(smsNotification, cancellationToken);
        }
    }

    private async Task SendWhatsAppForUser(MultiChannelNotification notification, CancellationToken cancellationToken)
    {
        // In a real implementation, look up user's WhatsApp number from database
        var userWhatsApp = await GetUserWhatsAppAsync(notification.UserId, cancellationToken);

        if (!string.IsNullOrEmpty(userWhatsApp))
        {
            var whatsappNotification = new WhatsAppNotification
            {
                PhoneNumber = userWhatsApp,
                Message = notification.Message,
                Priority = notification.Priority,
                Metadata = notification.Metadata
            };

            await SendWhatsAppAsync(whatsappNotification, cancellationToken);
        }
    }

    private async Task SendPushForUser(MultiChannelNotification notification, CancellationToken cancellationToken)
    {
        var pushNotification = new PushNotification
        {
            UserId = notification.UserId,
            Title = notification.Subject,
            Body = notification.Message,
            Priority = notification.Priority
        };

        await SendPushNotificationAsync(pushNotification, cancellationToken);
    }

    private async Task<string?> GetUserEmailAsync(string userId, CancellationToken cancellationToken)
    {
        // Placeholder - in real implementation, query the database
        await Task.CompletedTask;
        return $"user{userId}@harmoni360.com";
    }

    private async Task<string?> GetUserPhoneAsync(string userId, CancellationToken cancellationToken)
    {
        // Placeholder - in real implementation, query the database
        await Task.CompletedTask;
        return $"+6281234567{userId.PadLeft(3, '0')}";
    }

    private async Task<string?> GetUserWhatsAppAsync(string userId, CancellationToken cancellationToken)
    {
        // Placeholder - in real implementation, query the database
        await Task.CompletedTask;
        return await GetUserPhoneAsync(userId, cancellationToken);
    }

    public async Task SendSecurityIncidentNotificationAsync(int incidentId, string message, List<int> recipientIds, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending security incident notification for incident {IncidentId} to {RecipientCount} recipients", 
                incidentId, recipientIds.Count);

            var tasks = new List<Task>();

            foreach (var recipientId in recipientIds)
            {
                var notification = new MultiChannelNotification
                {
                    UserId = recipientId.ToString(),
                    Subject = $"Security Incident Alert - #{incidentId}",
                    Message = message,
                    Channels = new List<NotificationChannel> 
                    { 
                        NotificationChannel.Email, 
                        NotificationChannel.Push 
                    },
                    Priority = NotificationPriority.Critical,
                    Metadata = new Dictionary<string, string>
                    {
                        ["IncidentId"] = incidentId.ToString(),
                        ["Type"] = "SecurityIncident"
                    }
                };

                tasks.Add(SendMultiChannelAsync(notification, cancellationToken));
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Security incident notification sent successfully for incident {IncidentId}", incidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send security incident notification for incident {IncidentId}", incidentId);
            throw;
        }
    }

    public async Task SendSecurityEscalationNotificationAsync(int incidentId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending security escalation notification for incident {IncidentId}", incidentId);

            // In a real implementation, you would query for security managers/administrators
            var securityManagers = new List<string> { "1", "2" }; // Placeholder IDs

            var tasks = new List<Task>();

            foreach (var managerId in securityManagers)
            {
                var notification = new MultiChannelNotification
                {
                    UserId = managerId,
                    Subject = $"URGENT: Security Incident Escalated - #{incidentId}",
                    Message = $"Security incident #{incidentId} has been escalated. Reason: {reason}. Immediate attention required.",
                    Channels = new List<NotificationChannel> 
                    { 
                        NotificationChannel.Email, 
                        NotificationChannel.Sms, 
                        NotificationChannel.Push 
                    },
                    Priority = NotificationPriority.Emergency,
                    Metadata = new Dictionary<string, string>
                    {
                        ["IncidentId"] = incidentId.ToString(),
                        ["Type"] = "SecurityEscalation",
                        ["Reason"] = reason
                    }
                };

                tasks.Add(SendMultiChannelAsync(notification, cancellationToken));
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Security escalation notification sent successfully for incident {IncidentId}", incidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send security escalation notification for incident {IncidentId}", incidentId);
            throw;
        }
    }

    public async Task SendSecurityAssignmentNotificationAsync(int incidentId, int assigneeId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending security assignment notification for incident {IncidentId} to user {AssigneeId}", 
                incidentId, assigneeId);

            var notification = new MultiChannelNotification
            {
                UserId = assigneeId.ToString(),
                Subject = $"Security Incident Assigned - #{incidentId}",
                Message = $"You have been assigned to security incident #{incidentId}. Please review and take appropriate action.",
                Channels = new List<NotificationChannel> 
                { 
                    NotificationChannel.Email, 
                    NotificationChannel.Push 
                },
                Priority = NotificationPriority.High,
                Metadata = new Dictionary<string, string>
                {
                    ["IncidentId"] = incidentId.ToString(),
                    ["Type"] = "SecurityAssignment",
                    ["AssigneeId"] = assigneeId.ToString()
                }
            };

            await SendMultiChannelAsync(notification, cancellationToken);

            _logger.LogInformation("Security assignment notification sent successfully for incident {IncidentId}", incidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send security assignment notification for incident {IncidentId}", incidentId);
            throw;
        }
    }

    public void Dispose()
    {
        _smtpClient?.Dispose();
    }
}