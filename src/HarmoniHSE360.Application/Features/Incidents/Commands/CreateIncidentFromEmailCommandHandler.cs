using HarmoniHSE360.Application.Common.Interfaces;
using HarmoniHSE360.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace HarmoniHSE360.Application.Features.Incidents.Commands;

public class CreateIncidentFromEmailCommandHandler : IRequestHandler<CreateIncidentFromEmailCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateIncidentFromEmailCommandHandler> _logger;
    private readonly IFileStorageService _fileStorageService;

    public CreateIncidentFromEmailCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateIncidentFromEmailCommandHandler> logger,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    public async Task<int> Handle(CreateIncidentFromEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing incident report from email: {Email}", request.FromEmail);

        try
        {
            // Extract incident details from email content
            var incidentDetails = ParseEmailContent(request);
            
            // Find or create user based on email
            var user = await GetOrCreateUserFromEmail(request.FromEmail, request.FromName, cancellationToken);
            
            // Create incident
            var incident = Incident.Create(
                title: incidentDetails.Title,
                description: incidentDetails.Description,
                severity: incidentDetails.Severity,
                incidentDate: incidentDetails.IncidentDate,
                location: incidentDetails.Location,
                reporterName: user?.Name ?? request.FromName,
                reporterEmail: request.FromEmail,
                reporterDepartment: user?.Department ?? "External",
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

            // Process attachments
            foreach (var attachment in request.Attachments)
            {
                if (IsValidIncidentAttachment(attachment))
                {
                    try
                    {
                        using var stream = new MemoryStream(attachment.Content);
                        var uploadResult = await _fileStorageService.UploadAsync(
                            stream,
                            attachment.FileName,
                            attachment.ContentType,
                            "incident-attachments");

                        incident.AddAttachment(
                            attachment.FileName,
                            uploadResult.FilePath,
                            attachment.Size,
                            request.FromEmail);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to store attachment {FileName} for incident {IncidentId}", 
                            attachment.FileName, incident.Id);
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created incident {IncidentId} from email report", incident.Id);
            
            return incident.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create incident from email: {Email}", request.FromEmail);
            throw;
        }
    }

    private async Task<User?> GetOrCreateUserFromEmail(string email, string name, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null)
        {
            // For external users, we don't automatically create accounts
            // Instead we'll create incidents with external reporter info
            _logger.LogInformation("External incident report from unknown user: {Email}", email);
            return null;
        }

        return user;
    }

    private ParsedIncidentDetails ParseEmailContent(CreateIncidentFromEmailCommand request)
    {
        var details = new ParsedIncidentDetails
        {
            Title = ExtractTitle(request.Subject),
            Description = CleanEmailBody(request.Body),
            Severity = request.DetectedSeverity ?? DetectSeverityFromContent(request.Subject + " " + request.Body),
            IncidentDate = ExtractIncidentDate(request.Body) ?? request.ReceivedAt,
            Location = request.Location ?? ExtractLocation(request.Body) ?? "Location not specified",
            WitnessNames = ExtractWitnesses(request.Body),
            ImmediateActions = ExtractImmediateActions(request.Body)
        };

        return details;
    }

    private string ExtractTitle(string subject)
    {
        // Remove common email prefixes and clean up
        var title = Regex.Replace(subject, @"^(Re:|Fwd?:|FWD:|INCIDENT:|URGENT:)\s*", "", RegexOptions.IgnoreCase);
        return string.IsNullOrWhiteSpace(title) ? "Incident Report via Email" : title.Trim();
    }

    private string CleanEmailBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return "No description provided in email.";

        // Remove email signatures and quoted text
        var lines = body.Split('\n');
        var cleanLines = new List<string>();
        bool inQuotedSection = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip quoted text
            if (trimmedLine.StartsWith(">") || trimmedLine.StartsWith("On ") && trimmedLine.Contains(" wrote:"))
            {
                inQuotedSection = true;
                continue;
            }

            if (inQuotedSection && string.IsNullOrEmpty(trimmedLine))
                continue;

            inQuotedSection = false;

            // Skip signature separators
            if (trimmedLine == "--" || trimmedLine.StartsWith("---"))
                break;

            cleanLines.Add(line);
        }

        return string.Join('\n', cleanLines).Trim();
    }

    private IncidentSeverity DetectSeverityFromContent(string content)
    {
        var upperContent = content.ToUpperInvariant();

        if (upperContent.Contains("CRITICAL") || upperContent.Contains("EMERGENCY") || 
            upperContent.Contains("FATAL") || upperContent.Contains("DEATH"))
            return IncidentSeverity.Critical;

        if (upperContent.Contains("SERIOUS") || upperContent.Contains("SEVERE") || 
            upperContent.Contains("MAJOR") || upperContent.Contains("INJURY"))
            return IncidentSeverity.Serious;

        if (upperContent.Contains("MODERATE") || upperContent.Contains("MEDIUM"))
            return IncidentSeverity.Moderate;

        return IncidentSeverity.Minor;
    }

    private DateTime? ExtractIncidentDate(string body)
    {
        // Try to extract date patterns from email body
        var datePatterns = new[]
        {
            @"(?:occurred|happened|date|incident date)[:\s]*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
            @"(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
            @"(?:on|at)\s+(\w+ \d{1,2},? \d{4})"
        };

        foreach (var pattern in datePatterns)
        {
            var match = Regex.Match(body, pattern, RegexOptions.IgnoreCase);
            if (match.Success && DateTime.TryParse(match.Groups[1].Value, out var date))
            {
                return date;
            }
        }

        return null;
    }

    private string? ExtractLocation(string body)
    {
        var locationPatterns = new[]
        {
            @"(?:location|place|site|building|room)[:\s]*([^\r\n]+)",
            @"(?:at|in)\s+([A-Z][a-zA-Z\s]+(?:Building|Room|Floor|Lab|Office))",
            @"(?:building|room|floor|lab|office)\s*[:#]?\s*([^\r\n]+)"
        };

        foreach (var pattern in locationPatterns)
        {
            var match = Regex.Match(body, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return null;
    }

    private string? ExtractWitnesses(string body)
    {
        var witnessPatterns = new[]
        {
            @"(?:witness|witnesses|saw|observed)[:\s]*([^\r\n]+)",
            @"(?:present|witnessed by)[:\s]*([^\r\n]+)"
        };

        foreach (var pattern in witnessPatterns)
        {
            var match = Regex.Match(body, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return null;
    }

    private string? ExtractImmediateActions(string body)
    {
        var actionPatterns = new[]
        {
            @"(?:immediate actions?|first aid|response|action taken)[:\s]*([^\r\n]+)",
            @"(?:did|action|response)[:\s]*([^\r\n]+)"
        };

        foreach (var pattern in actionPatterns)
        {
            var match = Regex.Match(body, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return null;
    }

    private bool IsValidIncidentAttachment(EmailAttachmentDto attachment)
    {
        var allowedTypes = new[]
        {
            "image/jpeg", "image/jpg", "image/png", "image/gif",
            "application/pdf", "text/plain",
            "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };

        const long maxSize = 50 * 1024 * 1024; // 50MB

        return allowedTypes.Contains(attachment.ContentType.ToLowerInvariant()) &&
               attachment.Size <= maxSize &&
               attachment.Content.Length > 0;
    }

    private class ParsedIncidentDetails
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