using HarmoniHSE360.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HarmoniHSE360.Infrastructure.Services;

public class NotificationTemplateService : INotificationTemplateService
{
    private readonly ILogger<NotificationTemplateService> _logger;
    private readonly Dictionary<string, NotificationTemplate> _templates;

    public NotificationTemplateService(ILogger<NotificationTemplateService> logger)
    {
        _logger = logger;
        _templates = InitializeTemplates();
    }

    public async Task<NotificationTemplate?> GetTemplateAsync(string templateId, string language = "en", CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // For async interface compliance

        var key = $"{templateId}_{language}";
        if (_templates.TryGetValue(key, out var template))
        {
            return template;
        }

        // Fallback to English if specific language not found
        if (language != "en" && _templates.TryGetValue($"{templateId}_en", out var englishTemplate))
        {
            _logger.LogWarning("Template {TemplateId} not found for language {Language}, falling back to English",
                templateId, language);
            return englishTemplate;
        }

        _logger.LogWarning("Template {TemplateId} not found for any language", templateId);
        return null;
    }

    public async Task<string> RenderTemplateAsync(string templateId, Dictionary<string, object> data, string language = "en", CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateAsync(templateId, language, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Template {templateId} not found");
        }

        return RenderTemplate(template.BodyTemplate, data);
    }

    public async Task<NotificationContent> GenerateNotificationAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating notification content for template {TemplateId} in language {Language}",
                request.TemplateId, request.Language);

            var template = await GetTemplateAsync(request.TemplateId, request.Language, cancellationToken);
            if (template == null)
            {
                throw new InvalidOperationException($"Template {request.TemplateId} not found");
            }

            // Merge default values with provided data
            var mergedData = new Dictionary<string, object>(template.DefaultValues.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value));
            foreach (var kvp in request.Data)
            {
                mergedData[kvp.Key] = kvp.Value;
            }

            // Validate required fields
            var missingFields = template.RequiredFields.Where(field => !mergedData.ContainsKey(field)).ToList();
            if (missingFields.Any())
            {
                throw new InvalidOperationException($"Missing required fields for template {request.TemplateId}: {string.Join(", ", missingFields)}");
            }

            var content = new NotificationContent
            {
                Subject = RenderTemplate(template.SubjectTemplate, mergedData),
                Body = RenderTemplate(template.BodyTemplate, mergedData),
                HtmlBody = !string.IsNullOrEmpty(template.HtmlBodyTemplate)
                    ? RenderTemplate(template.HtmlBodyTemplate, mergedData)
                    : null,
                SmsMessage = !string.IsNullOrEmpty(template.SmsTemplate)
                    ? RenderTemplate(template.SmsTemplate, mergedData)
                    : RenderTemplate(template.BodyTemplate, mergedData),
                WhatsAppMessage = !string.IsNullOrEmpty(template.WhatsAppTemplate)
                    ? RenderTemplate(template.WhatsAppTemplate, mergedData)
                    : RenderTemplate(template.BodyTemplate, mergedData),
                PushTitle = !string.IsNullOrEmpty(template.PushTitleTemplate)
                    ? RenderTemplate(template.PushTitleTemplate, mergedData)
                    : RenderTemplate(template.SubjectTemplate, mergedData),
                PushBody = !string.IsNullOrEmpty(template.PushBodyTemplate)
                    ? RenderTemplate(template.PushBodyTemplate, mergedData)
                    : RenderTemplate(template.BodyTemplate, mergedData),
                Metadata = new Dictionary<string, string>
                {
                    ["template_id"] = request.TemplateId,
                    ["language"] = request.Language,
                    ["generated_at"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                }
            };

            _logger.LogInformation("Successfully generated notification content for template {TemplateId}", request.TemplateId);
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate notification content for template {TemplateId}", request.TemplateId);
            throw;
        }
    }

    private string RenderTemplate(string template, Dictionary<string, object> data)
    {
        if (string.IsNullOrEmpty(template))
            return string.Empty;

        var rendered = template;

        // Replace {{variable}} placeholders
        var regex = new Regex(@"\{\{(\w+)\}\}", RegexOptions.IgnoreCase);
        rendered = regex.Replace(rendered, match =>
        {
            var key = match.Groups[1].Value;
            if (data.TryGetValue(key, out var value))
            {
                return value?.ToString() ?? string.Empty;
            }

            _logger.LogWarning("Template variable {Variable} not found in data", key);
            return match.Value; // Return original placeholder if not found
        });

        return rendered;
    }

    private Dictionary<string, NotificationTemplate> InitializeTemplates()
    {
        var templates = new Dictionary<string, NotificationTemplate>();

        // Incident Created Templates
        templates.Add($"{NotificationTemplates.INCIDENT_CREATED_ID}_en", new NotificationTemplate
        {
            Id = NotificationTemplates.INCIDENT_CREATED_ID,
            Name = "Incident Created",
            Language = "en",
            Type = NotificationTemplateType.IncidentCreated,
            SubjectTemplate = "New Incident Reported: {{incident_title}}",
            BodyTemplate = "A new incident has been reported:\n\nIncident ID: {{incident_id}}\nTitle: {{incident_title}}\nSeverity: {{incident_severity}}\nLocation: {{incident_location}}\nReported by: {{reporter_name}}\nCreated: {{incident_created_at}}\n\nDescription:\n{{incident_description}}\n\nView details: {{url}}",
            HtmlBodyTemplate = "<h2>New Incident Reported</h2><p>A new incident has been reported:</p><ul><li><strong>Incident ID:</strong> {{incident_id}}</li><li><strong>Title:</strong> {{incident_title}}</li><li><strong>Severity:</strong> {{incident_severity}}</li><li><strong>Location:</strong> {{incident_location}}</li><li><strong>Reported by:</strong> {{reporter_name}}</li><li><strong>Created:</strong> {{incident_created_at}}</li></ul><p><strong>Description:</strong></p><p>{{incident_description}}</p><p><a href=\"{{url}}\">View Incident Details</a></p>",
            SmsTemplate = "New incident #{{incident_id}}: {{incident_title}}. Severity: {{incident_severity}}. Location: {{incident_location}}. View: {{url}}",
            PushTitleTemplate = "New Incident: {{incident_title}}",
            PushBodyTemplate = "Severity: {{incident_severity}} at {{incident_location}}",
            RequiredFields = new() { "incident_id", "incident_title", "incident_severity", "incident_location", "reporter_name", "incident_created_at", "incident_description", "url" }
        });

        templates.Add($"{NotificationTemplates.INCIDENT_CREATED_ID}_id", new NotificationTemplate
        {
            Id = NotificationTemplates.INCIDENT_CREATED_ID,
            Name = "Insiden Dibuat",
            Language = "id",
            Type = NotificationTemplateType.IncidentCreated,
            SubjectTemplate = "Insiden Baru Dilaporkan: {{incident_title}}",
            BodyTemplate = "Insiden baru telah dilaporkan:\n\nID Insiden: {{incident_id}}\nJudul: {{incident_title}}\nTingkat Keparahan: {{incident_severity}}\nLokasi: {{incident_location}}\nDilaporkan oleh: {{reporter_name}}\nDibuat: {{incident_created_at}}\n\nDeskripsi:\n{{incident_description}}\n\nLihat detail: {{url}}",
            SmsTemplate = "Insiden baru #{{incident_id}}: {{incident_title}}. Tingkat: {{incident_severity}}. Lokasi: {{incident_location}}. Lihat: {{url}}",
            PushTitleTemplate = "Insiden Baru: {{incident_title}}",
            PushBodyTemplate = "Tingkat: {{incident_severity}} di {{incident_location}}",
            RequiredFields = new() { "incident_id", "incident_title", "incident_severity", "incident_location", "reporter_name", "incident_created_at", "incident_description", "url" }
        });

        // Critical Incident Templates
        templates.Add($"{NotificationTemplates.INCIDENT_CRITICAL_ID}_en", new NotificationTemplate
        {
            Id = NotificationTemplates.INCIDENT_CRITICAL_ID,
            Name = "Critical Incident Alert",
            Language = "en",
            Type = NotificationTemplateType.IncidentEscalated,
            SubjectTemplate = "🚨 CRITICAL INCIDENT ALERT: {{incident_title}}",
            BodyTemplate = "🚨 CRITICAL INCIDENT REQUIRES IMMEDIATE ATTENTION 🚨\n\nIncident ID: {{incident_id}}\nTitle: {{incident_title}}\nSeverity: {{incident_severity}}\nLocation: {{incident_location}}\nReported by: {{reporter_name}}\nCreated: {{incident_created_at}}\n\nDescription:\n{{incident_description}}\n\n⚠️ THIS INCIDENT REQUIRES IMMEDIATE RESPONSE\n\nView details: {{url}}",
            SmsTemplate = "🚨 CRITICAL INCIDENT #{{incident_id}}: {{incident_title}} at {{incident_location}}. IMMEDIATE RESPONSE REQUIRED. View: {{url}}",
            PushTitleTemplate = "🚨 CRITICAL INCIDENT",
            PushBodyTemplate = "{{incident_title}} - IMMEDIATE RESPONSE REQUIRED",
            RequiredFields = new() { "incident_id", "incident_title", "incident_severity", "incident_location", "reporter_name", "incident_created_at", "incident_description", "url" }
        });

        // Escalation Overdue Templates
        templates.Add($"{NotificationTemplates.ESCALATION_OVERDUE_ID}_en", new NotificationTemplate
        {
            Id = NotificationTemplates.ESCALATION_OVERDUE_ID,
            Name = "Incident Escalation",
            Language = "en",
            Type = NotificationTemplateType.IncidentEscalated,
            SubjectTemplate = "Incident Escalated: {{incident_title}}",
            BodyTemplate = "An incident has been escalated and requires your attention:\n\nIncident ID: {{incident_id}}\nTitle: {{incident_title}}\nSeverity: {{incident_severity}}\nStatus: {{incident_status}}\nLocation: {{incident_location}}\nCreated: {{incident_created_at}}\n\nEscalation Reason: {{escalation_reason}}\nEscalated by: {{escalated_by}}\n\nDescription:\n{{incident_description}}\n\nPlease review and take appropriate action.\n\nView details: {{url}}",
            PushTitleTemplate = "Incident Escalated",
            PushBodyTemplate = "{{incident_title}} requires your attention",
            RequiredFields = new() { "incident_id", "incident_title", "incident_severity", "incident_status", "incident_location", "incident_created_at", "escalation_reason", "escalated_by", "incident_description", "url" }
        });

        // Emergency Alert Templates
        templates.Add($"{NotificationTemplates.EMERGENCY_ALERT_ID}_en", new NotificationTemplate
        {
            Id = NotificationTemplates.EMERGENCY_ALERT_ID,
            Name = "Emergency Alert",
            Language = "en",
            Type = NotificationTemplateType.EmergencyAlert,
            SubjectTemplate = "🚨 EMERGENCY ALERT: {{incident_title}}",
            BodyTemplate = "🚨 EMERGENCY SITUATION 🚨\n\nINCIDENT DETAILS:\nID: {{incident_id}}\nTitle: {{incident_title}}\nSeverity: {{incident_severity}}\nLocation: {{incident_location}}\nTime: {{incident_created_at}}\n\nDescription:\n{{incident_description}}\n\n🚨 ACTIVATE EMERGENCY RESPONSE PROCEDURES 🚨\n\nView details: {{url}}",
            SmsTemplate = "🚨 EMERGENCY: {{incident_title}} at {{incident_location}}. Incident #{{incident_id}}. ACTIVATE EMERGENCY PROCEDURES. {{url}}",
            PushTitleTemplate = "🚨 EMERGENCY ALERT",
            PushBodyTemplate = "{{incident_title}} - ACTIVATE EMERGENCY PROCEDURES",
            RequiredFields = new() { "incident_id", "incident_title", "incident_severity", "incident_location", "incident_created_at", "incident_description", "url" }
        });

        // Regulatory Reporting Templates
        templates.Add($"{NotificationTemplates.INCIDENT_REGULATORY_ID}_en", new NotificationTemplate
        {
            Id = NotificationTemplates.INCIDENT_REGULATORY_ID,
            Name = "Regulatory Reporting Required",
            Language = "en",
            Type = NotificationTemplateType.RegulatoryReport,
            SubjectTemplate = "Regulatory Reporting Required: {{incident_title}}",
            BodyTemplate = "A reportable incident has occurred that requires regulatory notification:\n\nIncident ID: {{incident_id}}\nTitle: {{incident_title}}\nSeverity: {{incident_severity}}\nLocation: {{incident_location}}\nCreated: {{incident_created_at}}\n\nDescription:\n{{incident_description}}\n\nRegulatory Requirements:\n- BPJS Ketenagakerjaan: 2x24 hours\n- Disnaker: As required\n- Local Authority: As required\n\nPlease prepare and submit the required regulatory reports.\n\nView details: {{url}}",
            RequiredFields = new() { "incident_id", "incident_title", "incident_severity", "incident_location", "incident_created_at", "incident_description", "url" }
        });

        // Investigator Assigned Templates
        templates.Add($"{NotificationTemplates.INVESTIGATOR_ASSIGNED_ID}_en", new NotificationTemplate
        {
            Id = NotificationTemplates.INVESTIGATOR_ASSIGNED_ID,
            Name = "Investigator Assigned",
            Language = "en",
            Type = NotificationTemplateType.InvestigatorAssigned,
            SubjectTemplate = "You have been assigned as investigator: {{incident_title}}",
            BodyTemplate = "You have been assigned as the investigator for the following incident:\n\nIncident ID: {{incident_id}}\nTitle: {{incident_title}}\nSeverity: {{incident_severity}}\nLocation: {{incident_location}}\nCreated: {{incident_created_at}}\n\nDescription:\n{{incident_description}}\n\nPlease begin your investigation and provide updates as required.\n\nView details: {{url}}",
            PushTitleTemplate = "Investigation Assignment",
            PushBodyTemplate = "You are assigned to investigate: {{incident_title}}",
            RequiredFields = new() { "incident_id", "incident_title", "incident_severity", "incident_location", "incident_created_at", "incident_description", "url" }
        });

        // Action Required Templates
        templates.Add($"{NotificationTemplates.ACTION_REQUIRED_ID}_en", new NotificationTemplate
        {
            Id = NotificationTemplates.ACTION_REQUIRED_ID,
            Name = "Action Required",
            Language = "en",
            Type = NotificationTemplateType.ActionRequired,
            SubjectTemplate = "Action Required: {{incident_title}}",
            BodyTemplate = "An action is required from you for the following incident:\n\nIncident ID: {{incident_id}}\nTitle: {{incident_title}}\nSeverity: {{incident_severity}}\nLocation: {{incident_location}}\nCreated: {{incident_created_at}}\n\nRequired Action: {{action_description}}\nDeadline: {{action_deadline}}\n\nDescription:\n{{incident_description}}\n\nPlease complete the required action by the deadline.\n\nView details: {{url}}",
            RequiredFields = new() { "incident_id", "incident_title", "incident_severity", "incident_location", "incident_created_at", "action_description", "action_deadline", "incident_description", "url" }
        });

        // Deadline Approaching Templates
        templates.Add($"{NotificationTemplates.DEADLINE_APPROACHING_ID}_en", new NotificationTemplate
        {
            Id = NotificationTemplates.DEADLINE_APPROACHING_ID,
            Name = "Deadline Approaching",
            Language = "en",
            Type = NotificationTemplateType.DeadlineApproaching,
            SubjectTemplate = "Deadline Approaching: {{task_type}} for {{incident_title}}",
            BodyTemplate = "A deadline is approaching for an incident task:\n\nIncident ID: {{incident_id}}\nTitle: {{incident_title}}\nTask Type: {{task_type}}\nDeadline: {{deadline}}\nTime Remaining: {{time_remaining}}\n\nPlease ensure the task is completed before the deadline.\n\nView details: {{url}}",
            SmsTemplate = "Deadline approaching: {{task_type}} for incident #{{incident_id}} due {{deadline}}. {{url}}",
            PushTitleTemplate = "Deadline Approaching",
            PushBodyTemplate = "{{task_type}} due {{deadline}}",
            RequiredFields = new() { "incident_id", "incident_title", "task_type", "deadline", "time_remaining", "url" }
        });

        // Incident Closed Templates
        templates.Add($"{NotificationTemplates.INCIDENT_CLOSED_ID}_en", new NotificationTemplate
        {
            Id = NotificationTemplates.INCIDENT_CLOSED_ID,
            Name = "Incident Closed",
            Language = "en",
            Type = NotificationTemplateType.IncidentClosed,
            SubjectTemplate = "Incident Closed: {{incident_title}}",
            BodyTemplate = "The following incident has been closed:\n\nIncident ID: {{incident_id}}\nTitle: {{incident_title}}\nSeverity: {{incident_severity}}\nLocation: {{incident_location}}\nCreated: {{incident_created_at}}\nClosed: {{incident_closed_at}}\nClosed by: {{closed_by}}\n\nResolution:\n{{resolution_summary}}\n\nView details: {{url}}",
            RequiredFields = new() { "incident_id", "incident_title", "incident_severity", "incident_location", "incident_created_at", "incident_closed_at", "closed_by", "resolution_summary", "url" }
        });

        // Weekly Digest Templates
        templates.Add($"{NotificationTemplates.WEEKLY_DIGEST_ID}_en", new NotificationTemplate
        {
            Id = NotificationTemplates.WEEKLY_DIGEST_ID,
            Name = "Weekly Incident Digest",
            Language = "en",
            Type = NotificationTemplateType.WeeklyDigest,
            SubjectTemplate = "Weekly HSE Incident Digest - Week of {{week_start_date}}",
            BodyTemplate = "Weekly HSE Incident Summary\nWeek: {{week_start_date}} to {{week_end_date}}\n\nSummary:\n- Total Incidents: {{total_incidents}}\n- New Incidents: {{new_incidents}}\n- Closed Incidents: {{closed_incidents}}\n- Open Incidents: {{open_incidents}}\n\nBy Severity:\n- Emergency: {{emergency_count}}\n- Critical: {{critical_count}}\n- Major: {{major_count}}\n- Minor: {{minor_count}}\n- Low: {{low_count}}\n\nView full report: {{report_url}}",
            RequiredFields = new() { "week_start_date", "week_end_date", "total_incidents", "new_incidents", "closed_incidents", "open_incidents", "emergency_count", "critical_count", "major_count", "minor_count", "low_count", "report_url" }
        });

        return templates;
    }
}