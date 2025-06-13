using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Events;
using Harmoni360.Domain.Common;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MediatR;
using EscalationActionType = Harmoni360.Domain.Entities.EscalationActionType;

namespace Harmoni360.Infrastructure.Services;

public class EscalationService : IEscalationService
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly INotificationTemplateService _templateService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<EscalationService> _logger;
    private readonly IMediator _mediator;

    public EscalationService(
        IApplicationDbContext context,
        INotificationService notificationService,
        INotificationTemplateService templateService,
        ICurrentUserService currentUserService,
        ILogger<EscalationService> logger,
        IMediator mediator)
    {
        _context = context;
        _notificationService = notificationService;
        _templateService = templateService;
        _currentUserService = currentUserService;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task ProcessEscalationRulesAsync(int incidentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing escalation rules for incident {IncidentId}", incidentId);

            var incident = await _context.Incidents
                .Include(i => i.InvolvedPersons)
                .FirstOrDefaultAsync(i => i.Id == incidentId, cancellationToken);

            if (incident == null)
            {
                _logger.LogWarning("Incident {IncidentId} not found for escalation processing", incidentId);
                return;
            }

            var applicableRules = await GetApplicableRulesAsync(incident, cancellationToken);

            foreach (var rule in applicableRules.OrderBy(r => r.Priority))
            {
                await ProcessEscalationRuleAsync(incident, rule, cancellationToken);
            }

            _logger.LogInformation("Completed processing {RuleCount} escalation rules for incident {IncidentId}",
                applicableRules.Count, incidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process escalation rules for incident {IncidentId}", incidentId);
            throw;
        }
    }

    public async Task CheckOverdueIncidentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking for overdue incidents");

            var overdueThreshold = DateTime.UtcNow.AddDays(-1); // 24 hours overdue
            var criticalThreshold = DateTime.UtcNow.AddHours(-2); // 2 hours for critical

            // Get incidents that are overdue for initial response
            var overdueIncidents = await _context.Incidents
                .Where(i => i.Status == IncidentStatus.Open || i.Status == IncidentStatus.InProgress)
                .Where(i => i.CreatedAt < overdueThreshold &&
                           (i.LastResponseAt == null || i.LastResponseAt < overdueThreshold))
                .ToListAsync(cancellationToken);

            // Get critical incidents that need immediate attention
            var criticalIncidents = await _context.Incidents
                .Where(i => i.Severity == IncidentSeverity.Critical || i.Severity == IncidentSeverity.Emergency)
                .Where(i => i.Status == IncidentStatus.Open)
                .Where(i => i.CreatedAt < criticalThreshold)
                .ToListAsync(cancellationToken);

            // Process overdue incidents
            foreach (var incident in overdueIncidents)
            {
                await TriggerOverdueEscalationAsync(incident, "24-hour response threshold exceeded", cancellationToken);
            }

            // Process critical incidents
            foreach (var incident in criticalIncidents)
            {
                await TriggerOverdueEscalationAsync(incident, "Critical incident requires immediate attention", cancellationToken);
            }

            _logger.LogInformation("Processed {OverdueCount} overdue and {CriticalCount} critical incidents",
                overdueIncidents.Count, criticalIncidents.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check overdue incidents");
            throw;
        }
    }

    public async Task TriggerManualEscalationAsync(int incidentId, string reason, string escalatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Triggering manual escalation for incident {IncidentId} by {EscalatedBy}",
                incidentId, escalatedBy);

            var incident = await _context.Incidents
                .Include(i => i.InvolvedPersons)
                .FirstOrDefaultAsync(i => i.Id == incidentId, cancellationToken);

            if (incident == null)
            {
                _logger.LogWarning("Incident {IncidentId} not found for manual escalation", incidentId);
                return;
            }

            // Get management escalation targets
            var managementTargets = await GetManagementTargetsAsync(incident, cancellationToken);

            // Create escalation event
            var escalationEvent = new EscalationTriggeredEvent(
                incidentId,
                "manual_escalation",
                reason,
                managementTargets);

            // Notify management
            await NotifyEscalationTargetsAsync(incident, managementTargets, reason, escalatedBy, cancellationToken);

            // Record escalation history
            await RecordEscalationHistoryAsync(incidentId, null, "Manual Escalation",
                EscalationActionType.EscalateToManager, string.Join(", ", managementTargets),
                reason, true, escalatedBy, cancellationToken);

            // Publish domain event
            await _mediator.Publish(escalationEvent, cancellationToken);

            _logger.LogInformation("Manual escalation completed for incident {IncidentId}", incidentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger manual escalation for incident {IncidentId}", incidentId);
            throw;
        }
    }

    public Task<List<EscalationRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default)
    {
        // In a real implementation, these would be stored in the database
        // For now, return predefined rules
        return Task.FromResult(new List<EscalationRule>
        {
            new EscalationRule
            {
                Name = "Critical Incident Immediate Escalation",
                Description = "Immediately escalate critical and emergency incidents",
                TriggerSeverities = new() { IncidentSeverity.Critical, IncidentSeverity.Emergency },
                Actions = new()
                {
                    new EscalationAction
                    {
                        Type = EscalationActionType.NotifyRole,
                        Target = "HSE_Manager",
                        TemplateId = NotificationTemplates.INCIDENT_CRITICAL_ID,
                        Channels = new() { NotificationChannel.Email, NotificationChannel.Sms, NotificationChannel.WhatsApp }
                    },
                    new EscalationAction
                    {
                        Type = EscalationActionType.SendEmergencyAlert,
                        Target = "emergency_team",
                        TemplateId = NotificationTemplates.EMERGENCY_ALERT_ID,
                        Channels = new() { NotificationChannel.Email, NotificationChannel.Sms, NotificationChannel.Push }
                    }
                },
                Priority = 1
            },
            new EscalationRule
            {
                Name = "24-Hour Response Escalation",
                Description = "Escalate incidents without response within 24 hours",
                TriggerAfterDuration = TimeSpan.FromHours(24),
                Actions = new()
                {
                    new EscalationAction
                    {
                        Type = EscalationActionType.EscalateToManager,
                        Target = "department_manager",
                        TemplateId = NotificationTemplates.ESCALATION_OVERDUE_ID,
                        Channels = new() { NotificationChannel.Email, NotificationChannel.Push }
                    }
                },
                Priority = 50
            },
            new EscalationRule
            {
                Name = "Regulatory Reporting",
                Description = "Trigger regulatory reporting for specific incident types",
                TriggerSeverities = new() { IncidentSeverity.Major, IncidentSeverity.Critical, IncidentSeverity.Emergency },
                Actions = new()
                {
                    new EscalationAction
                    {
                        Type = EscalationActionType.SendRegulatory,
                        Target = "regulatory_team",
                        TemplateId = NotificationTemplates.INCIDENT_REGULATORY_ID,
                        Channels = new() { NotificationChannel.Email },
                        Delay = TimeSpan.FromHours(2) // Give time for initial assessment
                    }
                },
                Priority = 75
            }
        });
    }

    private async Task<List<EscalationRule>> GetApplicableRulesAsync(Incident incident, CancellationToken cancellationToken)
    {
        var allRules = await GetActiveRulesAsync(cancellationToken);
        var applicableRules = new List<EscalationRule>();

        foreach (var rule in allRules)
        {
            bool isApplicable = true;

            // Check severity triggers
            if (rule.TriggerSeverities.Any() && !rule.TriggerSeverities.Contains(incident.Severity))
            {
                isApplicable = false;
            }

            // Check status triggers
            if (rule.TriggerStatuses.Any() && !rule.TriggerStatuses.Contains(incident.Status))
            {
                isApplicable = false;
            }

            // Check duration triggers
            if (rule.TriggerAfterDuration.HasValue)
            {
                var timeSinceCreation = DateTime.UtcNow - incident.CreatedAt;
                var timeSinceResponse = incident.LastResponseAt.HasValue
                    ? DateTime.UtcNow - incident.LastResponseAt.Value
                    : timeSinceCreation;

                if (timeSinceResponse < rule.TriggerAfterDuration.Value)
                {
                    isApplicable = false;
                }
            }

            // Check department triggers
            if (rule.TriggerDepartments.Any() && !string.IsNullOrEmpty(incident.Department))
            {
                if (!rule.TriggerDepartments.Contains(incident.Department))
                {
                    isApplicable = false;
                }
            }

            // Check location triggers
            if (rule.TriggerLocations.Any() && !string.IsNullOrEmpty(incident.Location))
            {
                if (!rule.TriggerLocations.Any(loc => incident.Location.Contains(loc, StringComparison.OrdinalIgnoreCase)))
                {
                    isApplicable = false;
                }
            }

            if (isApplicable)
            {
                applicableRules.Add(rule);
            }
        }

        return applicableRules;
    }

    private async Task ProcessEscalationRuleAsync(Incident incident, EscalationRule rule, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing escalation rule {RuleName} for incident {IncidentId}",
                rule.Name, incident.Id);

            foreach (var action in rule.Actions)
            {
                if (action.Delay.HasValue)
                {
                    // In a real implementation, you might use a background job scheduler
                    await Task.Delay(action.Delay.Value, cancellationToken);
                }

                await ExecuteEscalationActionAsync(incident, rule, action, cancellationToken);
            }

            // Create escalation event
            var escalationEvent = new EscalationTriggeredEvent(
                incident.Id,
                rule.Id.ToString(),
                rule.Description,
                rule.Actions.Select(a => a.Target).ToList());

            await _mediator.Publish(escalationEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process escalation rule {RuleName} for incident {IncidentId}",
                rule.Name, incident.Id);

            await RecordEscalationHistoryAsync(incident.Id, rule.Id, rule.Name,
                EscalationActionType.NotifyUser, "System", ex.Message, false, "system", cancellationToken);
        }
    }

    private async Task ExecuteEscalationActionAsync(Incident incident, EscalationRule rule, EscalationAction action, CancellationToken cancellationToken)
    {
        try
        {
            switch (action.Type)
            {
                case EscalationActionType.NotifyUser:
                    await NotifyUserAsync(incident, action, cancellationToken);
                    break;

                case EscalationActionType.NotifyRole:
                    await NotifyRoleAsync(incident, action, cancellationToken);
                    break;

                case EscalationActionType.NotifyDepartment:
                    await NotifyDepartmentAsync(incident, action, cancellationToken);
                    break;

                case EscalationActionType.EscalateToManager:
                    await EscalateToManagerAsync(incident, action, cancellationToken);
                    break;

                case EscalationActionType.SendEmergencyAlert:
                    await SendEmergencyAlertAsync(incident, action, cancellationToken);
                    break;

                case EscalationActionType.SendRegulatory:
                    await SendRegulatoryNotificationAsync(incident, action, cancellationToken);
                    break;

                default:
                    _logger.LogWarning("Unsupported escalation action type: {ActionType}", action.Type);
                    break;
            }

            await RecordEscalationHistoryAsync(incident.Id, rule.Id, rule.Name, action.Type,
                action.Target, "Action executed successfully", true, "system", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute escalation action {ActionType} for incident {IncidentId}",
                action.Type, incident.Id);

            await RecordEscalationHistoryAsync(incident.Id, rule.Id, rule.Name, action.Type,
                action.Target, ex.Message, false, "system", cancellationToken);
        }
    }

    private async Task NotifyUserAsync(Incident incident, EscalationAction action, CancellationToken cancellationToken)
    {
        var templateId = action.TemplateId ?? NotificationTemplates.INCIDENT_CREATED_ID;
        var data = CreateNotificationData(incident, action.Parameters);

        var notificationRequest = new NotificationRequest
        {
            TemplateId = templateId,
            Data = data,
            RecipientUserId = action.Target,
            PreferredChannels = action.Channels,
            Priority = NotificationPriority.High
        };

        var content = await _templateService.GenerateNotificationAsync(notificationRequest, cancellationToken);

        var multiChannelNotification = new MultiChannelNotification
        {
            UserId = action.Target,
            Subject = content.Subject,
            Message = content.Body,
            Channels = action.Channels,
            Priority = NotificationPriority.High
        };

        await _notificationService.SendMultiChannelAsync(multiChannelNotification, cancellationToken);
    }

    private async Task NotifyRoleAsync(Incident incident, EscalationAction action, CancellationToken cancellationToken)
    {
        // In a real implementation, query users with the specified role
        var usersInRole = await GetUsersInRoleAsync(action.Target, cancellationToken);

        foreach (var userId in usersInRole)
        {
            var userAction = new EscalationAction
            {
                Type = EscalationActionType.NotifyUser,
                Target = userId,
                TemplateId = action.TemplateId,
                Parameters = action.Parameters,
                Channels = action.Channels
            };

            await NotifyUserAsync(incident, userAction, cancellationToken);
        }
    }

    private async Task NotifyDepartmentAsync(Incident incident, EscalationAction action, CancellationToken cancellationToken)
    {
        // In a real implementation, query users in the specified department
        var usersInDepartment = await GetUsersInDepartmentAsync(action.Target, cancellationToken);

        foreach (var userId in usersInDepartment)
        {
            var userAction = new EscalationAction
            {
                Type = EscalationActionType.NotifyUser,
                Target = userId,
                TemplateId = action.TemplateId,
                Parameters = action.Parameters,
                Channels = action.Channels
            };

            await NotifyUserAsync(incident, userAction, cancellationToken);
        }
    }

    private async Task EscalateToManagerAsync(Incident incident, EscalationAction action, CancellationToken cancellationToken)
    {
        var managementTargets = await GetManagementTargetsAsync(incident, cancellationToken);

        foreach (var managerId in managementTargets)
        {
            var managerAction = new EscalationAction
            {
                Type = EscalationActionType.NotifyUser,
                Target = managerId,
                TemplateId = action.TemplateId ?? NotificationTemplates.ESCALATION_OVERDUE_ID,
                Parameters = action.Parameters,
                Channels = action.Channels
            };

            await NotifyUserAsync(incident, managerAction, cancellationToken);
        }
    }

    private async Task SendEmergencyAlertAsync(Incident incident, EscalationAction action, CancellationToken cancellationToken)
    {
        var emergencyEvent = new EmergencyAlertTriggeredEvent(
            incident.Id,
            incident.Severity.ToString(),
            incident.Location ?? "Unknown",
            new List<string> { "emergency_team", "site_safety_officer", "management" });

        await _mediator.Publish(emergencyEvent, cancellationToken);

        // Send immediate notifications to emergency contacts
        var emergencyContacts = await GetEmergencyContactsAsync(cancellationToken);

        foreach (var contact in emergencyContacts)
        {
            var emergencyAction = new EscalationAction
            {
                Type = EscalationActionType.NotifyUser,
                Target = contact,
                TemplateId = NotificationTemplates.EMERGENCY_ALERT_ID,
                Channels = new() { NotificationChannel.Email, NotificationChannel.Sms, NotificationChannel.Push }
            };

            await NotifyUserAsync(incident, emergencyAction, cancellationToken);
        }
    }

    private async Task SendRegulatoryNotificationAsync(Incident incident, EscalationAction action, CancellationToken cancellationToken)
    {
        var regulatoryEvent = new RegulatoryReportRequiredEvent(
            incident.Id,
            "BPJS_Ketenagakerjaan", // Indonesian work safety authority
            DateTime.UtcNow.AddHours(48), // 48-hour reporting requirement
            new List<string> { "BPJS_Ketenagakerjaan", "Disnaker", "Local_Authority" });

        await _mediator.Publish(regulatoryEvent, cancellationToken);

        // Notify regulatory team
        var regulatoryTeam = await GetRegulatoryTeamAsync(cancellationToken);

        foreach (var teamMember in regulatoryTeam)
        {
            var regulatoryAction = new EscalationAction
            {
                Type = EscalationActionType.NotifyUser,
                Target = teamMember,
                TemplateId = NotificationTemplates.INCIDENT_REGULATORY_ID,
                Channels = new() { NotificationChannel.Email }
            };

            await NotifyUserAsync(incident, regulatoryAction, cancellationToken);
        }
    }

    private async Task TriggerOverdueEscalationAsync(Incident incident, string reason, CancellationToken cancellationToken)
    {
        await TriggerManualEscalationAsync(incident.Id, reason, "system", cancellationToken);
    }

    private async Task NotifyEscalationTargetsAsync(Incident incident, List<string> targets, string reason, string escalatedBy, CancellationToken cancellationToken)
    {
        var data = CreateNotificationData(incident, new Dictionary<string, string>
        {
            ["escalation_reason"] = reason,
            ["escalated_by"] = escalatedBy
        });

        foreach (var target in targets)
        {
            var notificationRequest = new NotificationRequest
            {
                TemplateId = NotificationTemplates.ESCALATION_OVERDUE_ID,
                Data = data,
                RecipientUserId = target,
                PreferredChannels = new() { NotificationChannel.Email, NotificationChannel.Push },
                Priority = NotificationPriority.High
            };

            var content = await _templateService.GenerateNotificationAsync(notificationRequest, cancellationToken);

            var notification = new MultiChannelNotification
            {
                UserId = target,
                Subject = content.Subject,
                Message = content.Body,
                Channels = new() { NotificationChannel.Email, NotificationChannel.Push },
                Priority = NotificationPriority.High
            };

            await _notificationService.SendMultiChannelAsync(notification, cancellationToken);
        }
    }

    private async Task RecordEscalationHistoryAsync(int incidentId, int? ruleId, string ruleName,
        EscalationActionType actionType, string actionTarget, string actionDetails, bool isSuccessful,
        string executedBy, CancellationToken cancellationToken)
    {
        // In a real implementation, save to EscalationHistory table
        _logger.LogInformation("Escalation history: Incident={IncidentId}, Rule={RuleName}, Action={ActionType}, Target={ActionTarget}, Success={IsSuccessful}",
            incidentId, ruleName, actionType, actionTarget, isSuccessful);

        await Task.CompletedTask;
    }

    private Dictionary<string, object> CreateNotificationData(Incident incident, Dictionary<string, string> additionalParams)
    {
        var data = new Dictionary<string, object>
        {
            ["incident_id"] = incident.Id,
            ["incident_title"] = incident.Title,
            ["incident_description"] = incident.Description,
            ["incident_severity"] = incident.Severity.ToString(),
            ["incident_status"] = incident.Status.ToString(),
            ["incident_location"] = incident.Location ?? "Not specified",
            ["incident_department"] = incident.Department ?? "Not specified",
            ["incident_created_at"] = incident.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC"),
            ["reporter_name"] = incident.ReporterName ?? "Anonymous",
            ["url"] = $"https://harmoni360.com/incidents/{incident.Id}"
        };

        foreach (var param in additionalParams)
        {
            data[param.Key] = param.Value;
        }

        return data;
    }

    private async Task<List<string>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        // Placeholder - in real implementation, query the database
        await Task.CompletedTask;
        return roleName switch
        {
            "HSE_Manager" => new() { "hse_manager_1", "hse_manager_2" },
            "Safety_Officer" => new() { "safety_officer_1", "safety_officer_2", "safety_officer_3" },
            "Department_Manager" => new() { "dept_manager_1", "dept_manager_2" },
            _ => new()
        };
    }

    private async Task<List<string>> GetUsersInDepartmentAsync(string department, CancellationToken cancellationToken)
    {
        // Placeholder - in real implementation, query the database
        await Task.CompletedTask;
        return new() { $"{department}_manager", $"{department}_supervisor" };
    }

    private async Task<List<string>> GetManagementTargetsAsync(Incident incident, CancellationToken cancellationToken)
    {
        // Placeholder - in real implementation, query management hierarchy
        await Task.CompletedTask;
        return new() { "site_manager", "hse_manager", "operations_manager" };
    }

    private async Task<List<string>> GetEmergencyContactsAsync(CancellationToken cancellationToken)
    {
        // Placeholder - in real implementation, query emergency contact list
        await Task.CompletedTask;
        return new() { "emergency_coordinator", "site_safety_officer", "medical_officer" };
    }

    private async Task<List<string>> GetRegulatoryTeamAsync(CancellationToken cancellationToken)
    {
        // Placeholder - in real implementation, query regulatory team members
        await Task.CompletedTask;
        return new() { "regulatory_officer", "compliance_manager", "legal_counsel" };
    }
}