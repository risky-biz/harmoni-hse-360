using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Events;
using Harmoni360.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Harmoni360.Application.Features.Incidents.EventHandlers;

public class NotificationRequiredEventHandler : INotificationHandler<NotificationRequiredEvent>
{
    private readonly INotificationService _notificationService;
    private readonly INotificationTemplateService _templateService;
    private readonly ILogger<NotificationRequiredEventHandler> _logger;

    public NotificationRequiredEventHandler(
        INotificationService notificationService,
        INotificationTemplateService templateService,
        ILogger<NotificationRequiredEventHandler> logger)
    {
        _notificationService = notificationService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task Handle(NotificationRequiredEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing notification required event for incident {IncidentId} with template {TemplateId}",
                notification.IncidentId, notification.TemplateId);

            var notificationRequest = new NotificationRequest
            {
                TemplateId = notification.TemplateId,
                Data = notification.Data,
                RecipientUserId = notification.RecipientId,
                Priority = (NotificationPriority)(int)notification.Priority,
                PreferredChannels = new() { NotificationChannel.Email, NotificationChannel.Push }
            };

            var content = await _templateService.GenerateNotificationAsync(notificationRequest, cancellationToken);

            var multiChannelNotification = new MultiChannelNotification
            {
                UserId = notification.RecipientId,
                Subject = content.Subject,
                Message = content.Body,
                Channels = notificationRequest.PreferredChannels,
                Priority = (NotificationPriority)(int)notification.Priority
            };

            await _notificationService.SendMultiChannelAsync(multiChannelNotification, cancellationToken);

            _logger.LogInformation("Successfully processed notification for incident {IncidentId}", notification.IncidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process notification for incident {IncidentId}", notification.IncidentId);
            throw;
        }
    }
}

public class EscalationTriggeredEventHandler : INotificationHandler<EscalationTriggeredEvent>
{
    private readonly INotificationService _notificationService;
    private readonly INotificationTemplateService _templateService;
    private readonly ILogger<EscalationTriggeredEventHandler> _logger;

    public EscalationTriggeredEventHandler(
        INotificationService notificationService,
        INotificationTemplateService templateService,
        ILogger<EscalationTriggeredEventHandler> logger)
    {
        _notificationService = notificationService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task Handle(EscalationTriggeredEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing escalation triggered event for incident {IncidentId} with rule {EscalationRuleId}",
                notification.IncidentId, notification.EscalationRuleId);

            var escalationData = new Dictionary<string, object>
            {
                ["incident_id"] = notification.IncidentId,
                ["escalation_rule_id"] = notification.EscalationRuleId,
                ["escalation_reason"] = notification.Reason,
                ["escalated_at"] = notification.OccurredOn.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                ["url"] = $"https://harmoni360.com/incidents/{notification.IncidentId}"
            };

            foreach (var target in notification.EscalationTargets)
            {
                var notificationRequest = new NotificationRequest
                {
                    TemplateId = NotificationTemplates.ESCALATION_OVERDUE_ID,
                    Data = escalationData,
                    RecipientUserId = target,
                    Priority = NotificationPriority.High,
                    PreferredChannels = new() { NotificationChannel.Email, NotificationChannel.Push, NotificationChannel.Sms }
                };

                var content = await _templateService.GenerateNotificationAsync(notificationRequest, cancellationToken);

                var multiChannelNotification = new MultiChannelNotification
                {
                    UserId = target,
                    Subject = content.Subject,
                    Message = content.Body,
                    Channels = notificationRequest.PreferredChannels,
                    Priority = NotificationPriority.High
                };

                await _notificationService.SendMultiChannelAsync(multiChannelNotification, cancellationToken);
            }

            _logger.LogInformation("Successfully processed escalation notification for incident {IncidentId}", notification.IncidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process escalation notification for incident {IncidentId}", notification.IncidentId);
            throw;
        }
    }
}

public class RegulatoryReportRequiredEventHandler : INotificationHandler<RegulatoryReportRequiredEvent>
{
    private readonly INotificationService _notificationService;
    private readonly INotificationTemplateService _templateService;
    private readonly ILogger<RegulatoryReportRequiredEventHandler> _logger;

    public RegulatoryReportRequiredEventHandler(
        INotificationService notificationService,
        INotificationTemplateService templateService,
        ILogger<RegulatoryReportRequiredEventHandler> logger)
    {
        _notificationService = notificationService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task Handle(RegulatoryReportRequiredEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing regulatory report required event for incident {IncidentId}",
                notification.IncidentId);

            var regulatoryData = new Dictionary<string, object>
            {
                ["incident_id"] = notification.IncidentId,
                ["regulation_type"] = notification.RegulationType,
                ["deadline"] = notification.Deadline.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                ["authorities"] = string.Join(", ", notification.Authorities),
                ["occurred_on"] = notification.OccurredOn.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                ["url"] = $"https://harmoni360.com/incidents/{notification.IncidentId}"
            };

            // Notify regulatory team members
            var regulatoryTeam = new List<string> { "regulatory_officer", "compliance_manager", "legal_counsel" };

            foreach (var teamMember in regulatoryTeam)
            {
                var notificationRequest = new NotificationRequest
                {
                    TemplateId = NotificationTemplates.INCIDENT_REGULATORY_ID,
                    Data = regulatoryData,
                    RecipientUserId = teamMember,
                    Priority = NotificationPriority.High,
                    PreferredChannels = new() { NotificationChannel.Email }
                };

                var content = await _templateService.GenerateNotificationAsync(notificationRequest, cancellationToken);

                var emailNotification = new EmailNotification
                {
                    To = await GetUserEmailAsync(teamMember),
                    Subject = content.Subject,
                    Body = content.Body,
                    HtmlBody = content.HtmlBody,
                    Priority = NotificationPriority.High
                };

                await _notificationService.SendEmailAsync(emailNotification, cancellationToken);
            }

            _logger.LogInformation("Successfully processed regulatory notification for incident {IncidentId}", notification.IncidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process regulatory notification for incident {IncidentId}", notification.IncidentId);
            throw;
        }
    }

    private async Task<string> GetUserEmailAsync(string userId)
    {
        // Placeholder - in real implementation, query user's email from database
        await Task.CompletedTask;
        return $"{userId}@harmoni360.com";
    }
}

public class EmergencyAlertTriggeredEventHandler : INotificationHandler<EmergencyAlertTriggeredEvent>
{
    private readonly INotificationService _notificationService;
    private readonly INotificationTemplateService _templateService;
    private readonly ILogger<EmergencyAlertTriggeredEventHandler> _logger;

    public EmergencyAlertTriggeredEventHandler(
        INotificationService notificationService,
        INotificationTemplateService templateService,
        ILogger<EmergencyAlertTriggeredEventHandler> logger)
    {
        _notificationService = notificationService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task Handle(EmergencyAlertTriggeredEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing emergency alert triggered event for incident {IncidentId}",
                notification.IncidentId);

            var emergencyData = new Dictionary<string, object>
            {
                ["incident_id"] = notification.IncidentId,
                ["alert_level"] = notification.AlertLevel,
                ["location"] = notification.Location,
                ["occurred_on"] = notification.OccurredOn.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                ["emergency_contacts"] = string.Join(", ", notification.EmergencyContacts),
                ["url"] = $"https://harmoni360.com/incidents/{notification.IncidentId}"
            };

            // Send immediate high-priority notifications to all emergency contacts
            foreach (var contact in notification.EmergencyContacts)
            {
                var emergencyContactsList = await GetEmergencyContactListAsync(contact);

                foreach (var contactId in emergencyContactsList)
                {
                    var notificationRequest = new NotificationRequest
                    {
                        TemplateId = NotificationTemplates.EMERGENCY_ALERT_ID,
                        Data = emergencyData,
                        RecipientUserId = contactId,
                        Priority = NotificationPriority.Emergency,
                        PreferredChannels = new() { NotificationChannel.Email, NotificationChannel.Sms, NotificationChannel.Push, NotificationChannel.WhatsApp }
                    };

                    var content = await _templateService.GenerateNotificationAsync(notificationRequest, cancellationToken);

                    var multiChannelNotification = new MultiChannelNotification
                    {
                        UserId = contactId,
                        Subject = content.Subject,
                        Message = content.Body,
                        Channels = notificationRequest.PreferredChannels,
                        Priority = NotificationPriority.Emergency,
                        DelayBetweenChannels = TimeSpan.Zero // Send all channels immediately
                    };

                    await _notificationService.SendMultiChannelAsync(multiChannelNotification, cancellationToken);
                }
            }

            _logger.LogInformation("Successfully processed emergency alert for incident {IncidentId}", notification.IncidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process emergency alert for incident {IncidentId}", notification.IncidentId);
            throw;
        }
    }

    private async Task<List<string>> GetEmergencyContactListAsync(string contactGroup)
    {
        // Placeholder - in real implementation, query emergency contacts from database
        await Task.CompletedTask;

        return contactGroup switch
        {
            "emergency_team" => new() { "emergency_coordinator", "site_safety_officer", "medical_officer" },
            "site_safety_officer" => new() { "safety_officer_1", "safety_officer_2" },
            "management" => new() { "site_manager", "operations_manager", "hse_manager" },
            _ => new() { contactGroup }
        };
    }
}

public class DeadlineApproachingEventHandler : INotificationHandler<DeadlineApproachingEvent>
{
    private readonly INotificationService _notificationService;
    private readonly INotificationTemplateService _templateService;
    private readonly ILogger<DeadlineApproachingEventHandler> _logger;

    public DeadlineApproachingEventHandler(
        INotificationService notificationService,
        INotificationTemplateService templateService,
        ILogger<DeadlineApproachingEventHandler> logger)
    {
        _notificationService = notificationService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task Handle(DeadlineApproachingEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing deadline approaching event for incident {IncidentId}",
                notification.IncidentId);

            var deadlineData = new Dictionary<string, object>
            {
                ["incident_id"] = notification.IncidentId,
                ["task_type"] = notification.TaskType,
                ["deadline"] = notification.Deadline.ToString("yyyy-MM-dd HH:mm:ss"),
                ["time_remaining"] = FormatTimeRemaining(notification.TimeRemaining),
                ["assigned_to_id"] = notification.AssignedToId,
                ["url"] = $"https://harmoni360.com/incidents/{notification.IncidentId}"
            };

            var notificationRequest = new NotificationRequest
            {
                TemplateId = NotificationTemplates.DEADLINE_APPROACHING_ID,
                Data = deadlineData,
                RecipientUserId = notification.AssignedToId,
                Priority = NotificationPriority.High,
                PreferredChannels = new() { NotificationChannel.Email, NotificationChannel.Push }
            };

            var content = await _templateService.GenerateNotificationAsync(notificationRequest, cancellationToken);

            var multiChannelNotification = new MultiChannelNotification
            {
                UserId = notification.AssignedToId,
                Subject = content.Subject,
                Message = content.Body,
                Channels = notificationRequest.PreferredChannels,
                Priority = NotificationPriority.High
            };

            await _notificationService.SendMultiChannelAsync(multiChannelNotification, cancellationToken);

            _logger.LogInformation("Successfully processed deadline notification for incident {IncidentId}", notification.IncidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process deadline notification for incident {IncidentId}", notification.IncidentId);
            throw;
        }
    }

    private string FormatTimeRemaining(TimeSpan timeRemaining)
    {
        if (timeRemaining.TotalDays >= 1)
            return $"{timeRemaining.Days} day(s)";

        if (timeRemaining.TotalHours >= 1)
            return $"{timeRemaining.Hours} hour(s)";

        return $"{timeRemaining.Minutes} minute(s)";
    }
}