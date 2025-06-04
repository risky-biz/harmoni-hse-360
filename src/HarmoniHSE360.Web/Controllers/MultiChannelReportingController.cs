using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using HarmoniHSE360.Application.Features.Incidents.Commands;
using HarmoniHSE360.Domain.Entities;
using Microsoft.AspNetCore.RateLimiting;

namespace HarmoniHSE360.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MultiChannelReportingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MultiChannelReportingController> _logger;

    public MultiChannelReportingController(
        IMediator mediator,
        ILogger<MultiChannelReportingController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Quick incident reporting (supports QR code and mobile)
    /// </summary>
    [HttpPost("quick-report")]
    [AllowAnonymous] // Allow anonymous reporting
    [EnableRateLimiting("QuickReportPolicy")]
    [ProducesResponseType(typeof(QuickReportResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<QuickReportResponse>> CreateQuickReport([FromBody] QuickReportRequest request)
    {
        try
        {
            _logger.LogInformation("Quick report received via {Channel} from {Reporter}", 
                request.ReportingChannel, request.IsAnonymous ? "Anonymous" : request.ReporterEmail);

            var command = new CreateQuickIncidentCommand
            {
                UserId = request.UserId,
                ReporterName = request.ReporterName,
                ReporterEmail = request.ReporterEmail,
                Title = request.Title,
                Description = request.Description,
                Severity = Enum.Parse<IncidentSeverity>(request.Severity),
                IncidentDate = request.IncidentDate ?? DateTime.UtcNow,
                Location = request.Location,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                QrCodeId = request.QrCodeId,
                ReportingChannel = request.ReportingChannel,
                IsAnonymous = request.IsAnonymous,
                DeviceInfo = request.DeviceInfo,
                PhotoBase64 = request.PhotoBase64 ?? new List<string>()
            };

            var incidentId = await _mediator.Send(command);

            var response = new QuickReportResponse
            {
                IncidentId = incidentId,
                Message = "Incident report received successfully",
                ReferenceNumber = $"QR-{DateTime.UtcNow:yyyyMMdd}-{incidentId:D6}",
                NextSteps = GenerateNextStepsMessage(request.ReportingChannel, request.IsAnonymous)
            };

            return CreatedAtAction(nameof(GetQuickReportStatus), new { id = incidentId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing quick report");
            return BadRequest(new { message = "Failed to process incident report" });
        }
    }

    /// <summary>
    /// Anonymous incident reporting portal
    /// </summary>
    [HttpPost("anonymous")]
    [AllowAnonymous]
    [EnableRateLimiting("AnonymousReportPolicy")]
    [ProducesResponseType(typeof(AnonymousReportResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AnonymousReportResponse>> CreateAnonymousReport([FromBody] AnonymousReportRequest request)
    {
        try
        {
            _logger.LogInformation("Anonymous report received from IP {IP}", HttpContext.Connection.RemoteIpAddress);

            var command = new CreateQuickIncidentCommand
            {
                ReporterName = "Anonymous Reporter",
                ReporterEmail = $"anonymous_{Guid.NewGuid():N}@harmoniHSE360.anonymous",
                Title = request.Title,
                Description = request.Description,
                Severity = Enum.Parse<IncidentSeverity>(request.Severity),
                IncidentDate = request.IncidentDate ?? DateTime.UtcNow,
                Location = request.Location,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                ReportingChannel = "Anonymous Portal",
                IsAnonymous = true,
                DeviceInfo = Request.Headers.UserAgent.ToString()
            };

            var incidentId = await _mediator.Send(command);

            var response = new AnonymousReportResponse
            {
                Success = true,
                Message = "Your anonymous report has been received and will be investigated promptly",
                ReferenceNumber = $"ANON-{DateTime.UtcNow:yyyyMMdd}-{incidentId:D6}",
                ContactInfo = "If you need to provide additional information, please contact HSE@school.edu"
            };

            return CreatedAtAction(nameof(GetQuickReportStatus), new { id = incidentId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing anonymous report");
            return BadRequest(new { message = "Failed to process anonymous report" });
        }
    }

    /// <summary>
    /// Email-to-incident webhook (for email gateway integration)
    /// </summary>
    [HttpPost("email-webhook")]
    [AllowAnonymous] // Should be secured with API key in production
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessEmailReport([FromBody] EmailWebhookRequest request)
    {
        try
        {
            _logger.LogInformation("Email report received from {Email}", request.FromEmail);

            var command = new CreateIncidentFromEmailCommand
            {
                FromEmail = request.FromEmail,
                FromName = request.FromName,
                Subject = request.Subject,
                Body = request.Body,
                Attachments = request.Attachments.Select(a => new EmailAttachmentDto
                {
                    FileName = a.FileName,
                    Content = Convert.FromBase64String(a.ContentBase64),
                    ContentType = a.ContentType,
                    Size = a.Size
                }).ToList(),
                ReceivedAt = request.ReceivedAt,
                Location = request.ExtractedLocation,
                DetectedSeverity = request.DetectedSeverity
            };

            var incidentId = await _mediator.Send(command);

            return Ok(new { incidentId, message = "Email processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email report from {Email}", request.FromEmail);
            return BadRequest(new { message = "Failed to process email report" });
        }
    }

    /// <summary>
    /// WhatsApp webhook (for WhatsApp Business API integration)
    /// </summary>
    [HttpPost("whatsapp-webhook")]
    [AllowAnonymous] // Should be secured with WhatsApp verification token
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessWhatsAppReport([FromBody] WhatsAppWebhookRequest request)
    {
        try
        {
            _logger.LogInformation("WhatsApp report received from {Phone}", request.FromPhoneNumber);

            var command = new CreateIncidentFromWhatsAppCommand
            {
                FromPhoneNumber = request.FromPhoneNumber,
                FromName = request.FromName,
                MessageBody = request.MessageBody,
                MediaAttachments = request.MediaAttachments.Select(m => new WhatsAppMediaDto
                {
                    MediaId = m.MediaId,
                    MediaType = m.MediaType,
                    Caption = m.Caption,
                    MediaData = !string.IsNullOrEmpty(m.MediaDataBase64) ? 
                        Convert.FromBase64String(m.MediaDataBase64) : null,
                    FileName = m.FileName,
                    MimeType = m.MimeType,
                    FileSize = m.FileSize
                }).ToList(),
                ReceivedAt = request.ReceivedAt,
                MessageId = request.MessageId,
                ChatId = request.ChatId
            };

            var incidentId = await _mediator.Send(command);

            return Ok(new { incidentId, message = "WhatsApp message processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WhatsApp report from {Phone}", request.FromPhoneNumber);
            return BadRequest(new { message = "Failed to process WhatsApp report" });
        }
    }

    /// <summary>
    /// Get the status of a quick report
    /// </summary>
    [HttpGet("status/{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReportStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult<ReportStatusResponse>> GetQuickReportStatus(int id)
    {
        try
        {
            // This would typically fetch incident status
            // For now, return a basic response
            var response = new ReportStatusResponse
            {
                IncidentId = id,
                Status = "Received",
                Message = "Your report has been received and is being processed",
                LastUpdated = DateTime.UtcNow
            };

            return Task.FromResult<ActionResult<ReportStatusResponse>>(Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report status for ID {Id}", id);
            return Task.FromResult<ActionResult<ReportStatusResponse>>(NotFound());
        }
    }

    /// <summary>
    /// Generate QR codes for location-based reporting
    /// </summary>
    [HttpPost("generate-qr")]
    [Authorize(Roles = "Admin,HSEManager")]
    [ProducesResponseType(typeof(QrCodeResponse), StatusCodes.Status200OK)]
    public Task<ActionResult<QrCodeResponse>> GenerateLocationQrCode([FromBody] QrCodeGenerationRequest request)
    {
        try
        {
            var qrCodeId = Guid.NewGuid().ToString("N");
            var qrCodeUrl = $"{Request.Scheme}://{Request.Host}/report/qr/{qrCodeId}";

            // Here you would typically:
            // 1. Store the QR code location mapping
            // 2. Generate the actual QR code image
            // 3. Return the QR code data

            var response = new QrCodeResponse
            {
                QrCodeId = qrCodeId,
                QrCodeUrl = qrCodeUrl,
                LocationName = request.LocationName,
                QrCodeImageBase64 = GenerateQrCodeImage(qrCodeUrl), // This would use a QR code library
                Message = "QR code generated successfully"
            };

            return Task.FromResult<ActionResult<QrCodeResponse>>(Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code for location {Location}", request.LocationName);
            return Task.FromResult<ActionResult<QrCodeResponse>>(BadRequest(new { message = "Failed to generate QR code" }));
        }
    }

    private string GenerateNextStepsMessage(string channel, bool isAnonymous)
    {
        var message = "Your incident report has been successfully submitted. ";
        
        if (isAnonymous)
        {
            message += "As this is an anonymous report, updates will not be sent directly to you. ";
        }
        else
        {
            message += "You will receive email updates about the investigation progress. ";
        }

        message += channel switch
        {
            "QR" => "Thank you for using our QR code reporting system.",
            "WhatsApp" => "You can reply to this WhatsApp conversation for additional information.",
            "Email" => "You can reply to the confirmation email for additional information.",
            _ => "The HSE team will review your report promptly."
        };

        return message;
    }

    private string GenerateQrCodeImage(string url)
    {
        // This would typically use a QR code generation library
        // For now, return a placeholder
        return "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";
    }
}

// Request/Response models
public class QuickReportRequest
{
    public string? UserId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string ReporterEmail { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Minor";
    public DateTime? IncidentDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? QrCodeId { get; set; }
    public string ReportingChannel { get; set; } = "Web";
    public bool IsAnonymous { get; set; } = false;
    public string? DeviceInfo { get; set; }
    public List<string>? PhotoBase64 { get; set; }
}

public class QuickReportResponse
{
    public int IncidentId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string NextSteps { get; set; } = string.Empty;
}

public class AnonymousReportRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Minor";
    public DateTime? IncidentDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class AnonymousReportResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
}

public class EmailWebhookRequest
{
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<EmailAttachmentRequest> Attachments { get; set; } = new();
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public string? ExtractedLocation { get; set; }
    public IncidentSeverity? DetectedSeverity { get; set; }
}

public class EmailAttachmentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string ContentBase64 { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
}

public class WhatsAppWebhookRequest
{
    public string FromPhoneNumber { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;
    public List<WhatsAppMediaRequest> MediaAttachments { get; set; } = new();
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public string? MessageId { get; set; }
    public string? ChatId { get; set; }
}

public class WhatsAppMediaRequest
{
    public string MediaId { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string? MediaDataBase64 { get; set; }
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public long? FileSize { get; set; }
}

public class ReportStatusResponse
{
    public int IncidentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public class QrCodeGenerationRequest
{
    public string LocationName { get; set; } = string.Empty;
    public string? BuildingId { get; set; }
    public string? FloorLevel { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Description { get; set; }
}

public class QrCodeResponse
{
    public string QrCodeId { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string QrCodeImageBase64 { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}