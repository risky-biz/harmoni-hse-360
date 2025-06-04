using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace HarmoniHSE360.Application.Features.Incidents.Commands;

public class CreateIncidentFromWhatsAppCommandHandler : IRequestHandler<CreateIncidentFromWhatsAppCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateIncidentFromWhatsAppCommandHandler> _logger;
    private readonly IFileStorageService _fileStorageService;

    public CreateIncidentFromWhatsAppCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateIncidentFromWhatsAppCommandHandler> logger,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    public async Task<int> Handle(CreateIncidentFromWhatsAppCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing incident report from WhatsApp: {PhoneNumber}", request.FromPhoneNumber);

        try
        {
            // Parse WhatsApp message content
            var incidentDetails = ParseWhatsAppMessage(request);
            
            // Find user by phone number or create external reporter
            var user = await GetUserByPhoneNumber(request.FromPhoneNumber, cancellationToken);
            
            // Create incident
            var incident = Incident.Create(
                title: incidentDetails.Title,
                description: incidentDetails.Description,
                severity: incidentDetails.Severity,
                incidentDate: incidentDetails.IncidentDate,
                location: incidentDetails.Location,
                reporterName: user?.Name ?? request.FromName,
                reporterEmail: user?.Email ?? GeneratePhoneBasedEmail(request.FromPhoneNumber),
                reporterDepartment: user?.Department ?? "External/WhatsApp",
                reporterId: user?.Id
            );

            // Add witness information if detected
            if (!string.IsNullOrEmpty(incidentDetails.WitnessNames))
            {
                incident.AddWitnessInformation(incidentDetails.WitnessNames);
            }

            // Add immediate actions if detected
            if (!string.IsNullOrEmpty(incidentDetails.ImmediateActions))
            {
                incident.RecordImmediateActions(incidentDetails.ImmediateActions);
            }

            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync(cancellationToken);

            // Process media attachments
            foreach (var media in request.MediaAttachments)
            {
                if (IsValidWhatsAppMedia(media))
                {
                    try
                    {
                        await ProcessWhatsAppMedia(incident, media, request.FromPhoneNumber, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to process WhatsApp media {MediaId} for incident {IncidentId}", 
                            media.MediaId, incident.Id);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created incident {IncidentId} from WhatsApp report", incident.Id);
            
            return incident.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create incident from WhatsApp: {PhoneNumber}", request.FromPhoneNumber);
            throw;
        }
    }

    private Task<User?> GetUserByPhoneNumber(string phoneNumber, CancellationToken cancellationToken)
    {
        // Note: This would require adding phone number field to User entity
        // For now, we'll try to match by a pattern or return null for external users
        
        // Clean phone number
        var cleanPhone = Regex.Replace(phoneNumber, @"[^\d]", "");
        
        // Try to find user by phone number (this would need to be implemented in the User entity)
        // For now, return null to treat all WhatsApp reports as external
        return Task.FromResult<User?>(null);
    }

    private ParsedWhatsAppIncident ParseWhatsAppMessage(CreateIncidentFromWhatsAppCommand request)
    {
        var messageBody = request.MessageBody;
        
        var details = new ParsedWhatsAppIncident
        {
            Title = ExtractTitleFromWhatsApp(messageBody),
            Description = EnhanceWhatsAppDescription(request),
            Severity = DetectSeverityFromWhatsApp(messageBody),
            IncidentDate = ExtractDateFromWhatsApp(messageBody) ?? request.ReceivedAt,
            Location = ExtractLocationFromWhatsApp(messageBody),
            WitnessNames = ExtractWitnessesFromWhatsApp(messageBody),
            ImmediateActions = ExtractActionsFromWhatsApp(messageBody)
        };

        return details;
    }

    private string ExtractTitleFromWhatsApp(string message)
    {
        // Look for explicit title patterns
        var titlePatterns = new[]
        {
            @"(?:title|subject|incident)[:\s]*([^\r\n]+)",
            @"^([^\r\n]{10,60})" // First line if it's a reasonable length
        };

        foreach (var pattern in titlePatterns)
        {
            var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (match.Success && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
            {
                return match.Groups[1].Value.Trim();
            }
        }

        // Fallback: create title from first words
        var words = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length > 0)
        {
            var title = string.Join(" ", words.Take(8));
            return title.Length > 50 ? title.Substring(0, 47) + "..." : title;
        }

        return "WhatsApp Incident Report";
    }

    private string EnhanceWhatsAppDescription(CreateIncidentFromWhatsAppCommand request)
    {
        var description = request.MessageBody;

        // Add WhatsApp metadata
        description += $"\n\n--- WhatsApp Report Details ---";
        description += $"\nFrom: {request.FromName} ({request.FromPhoneNumber})";
        description += $"\nReceived: {request.ReceivedAt:yyyy-MM-dd HH:mm:ss} UTC";
        
        if (!string.IsNullOrEmpty(request.MessageId))
            description += $"\nMessage ID: {request.MessageId}";
        
        if (request.MediaAttachments.Any())
        {
            description += $"\nMedia Attachments: {request.MediaAttachments.Count}";
            foreach (var media in request.MediaAttachments)
            {
                description += $"\n- {media.MediaType}";
                if (!string.IsNullOrEmpty(media.Caption))
                    description += $": {media.Caption}";
            }
        }

        return description;
    }

    private IncidentSeverity DetectSeverityFromWhatsApp(string message)
    {
        var upperMessage = message.ToUpperInvariant();

        // Check for urgency indicators
        if (upperMessage.Contains("URGENT") || upperMessage.Contains("EMERGENCY") || 
            upperMessage.Contains("CRITICAL") || upperMessage.Contains("!!!"))
            return IncidentSeverity.Critical;

        if (upperMessage.Contains("SERIOUS") || upperMessage.Contains("INJURY") || 
            upperMessage.Contains("HURT") || upperMessage.Contains("BLOOD"))
            return IncidentSeverity.Serious;

        if (upperMessage.Contains("MODERATE") || upperMessage.Contains("CONCERN"))
            return IncidentSeverity.Moderate;

        return IncidentSeverity.Minor;
    }

    private DateTime? ExtractDateFromWhatsApp(string message)
    {
        // Similar to email date extraction but more casual
        var datePatterns = new[]
        {
            @"(?:today|earlier|now|just happened)",
            @"(?:yesterday)",
            @"(\d{1,2}[/-]\d{1,2})",
            @"(?:this morning|this afternoon|this evening)"
        };

        var now = DateTime.UtcNow;

        foreach (var pattern in datePatterns)
        {
            var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var matchValue = match.Value.ToLowerInvariant();
                
                if (matchValue.Contains("today") || matchValue.Contains("now") || matchValue.Contains("just"))
                    return now;
                
                if (matchValue.Contains("yesterday"))
                    return now.AddDays(-1);
                
                if (matchValue.Contains("morning"))
                    return now.Date.AddHours(9);
                
                if (matchValue.Contains("afternoon"))
                    return now.Date.AddHours(14);
                
                if (matchValue.Contains("evening"))
                    return now.Date.AddHours(19);
            }
        }

        return null;
    }

    private string ExtractLocationFromWhatsApp(string message)
    {
        var locationPatterns = new[]
        {
            @"(?:at|in|location|place)[:\s]*([^\r\n]+)",
            @"(?:building|room|office|lab|floor)\s*[:#]?\s*([^\r\n]+)"
        };

        foreach (var pattern in locationPatterns)
        {
            var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return "Location not specified in WhatsApp message";
    }

    private string? ExtractWitnessesFromWhatsApp(string message)
    {
        var witnessPatterns = new[]
        {
            @"(?:saw|witness|people present)[:\s]*([^\r\n]+)",
            @"(?:with me|others)[:\s]*([^\r\n]+)"
        };

        foreach (var pattern in witnessPatterns)
        {
            var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return null;
    }

    private string? ExtractActionsFromWhatsApp(string message)
    {
        var actionPatterns = new[]
        {
            @"(?:did|action|help|first aid)[:\s]*([^\r\n]+)",
            @"(?:called|contacted|notified)[:\s]*([^\r\n]+)"
        };

        foreach (var pattern in actionPatterns)
        {
            var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return null;
    }

    private string GeneratePhoneBasedEmail(string phoneNumber)
    {
        var cleanPhone = Regex.Replace(phoneNumber, @"[^\d]", "");
        return $"whatsapp_{cleanPhone}@harmoniHSE360.external";
    }

    private bool IsValidWhatsAppMedia(WhatsAppMediaDto media)
    {
        var allowedTypes = new[] { "image", "video", "document" };
        const long maxSize = 100 * 1024 * 1024; // 100MB for WhatsApp

        return allowedTypes.Contains(media.MediaType.ToLowerInvariant()) &&
               (media.FileSize ?? 0) <= maxSize &&
               media.MediaData != null &&
               media.MediaData.Length > 0;
    }

    private async Task ProcessWhatsAppMedia(Incident incident, WhatsAppMediaDto media, string reporterPhone, CancellationToken cancellationToken)
    {
        if (media.MediaData == null) return;

        var fileName = media.FileName ?? $"whatsapp_media_{media.MediaId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
        var contentType = media.MimeType ?? $"{media.MediaType}/octet-stream";

        using var stream = new MemoryStream(media.MediaData);
        var uploadResult = await _fileStorageService.UploadAsync(
            stream,
            fileName,
            contentType,
            "whatsapp-media");

        incident.AddAttachment(
            fileName,
            uploadResult.FilePath,
            media.MediaData.Length,
            $"whatsapp_{reporterPhone}");
    }

    private class ParsedWhatsAppIncident
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IncidentSeverity Severity { get; set; }
        public DateTime IncidentDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? WitnessNames { get; set; }
        public string? ImmediateActions { get; set; }
    }
}