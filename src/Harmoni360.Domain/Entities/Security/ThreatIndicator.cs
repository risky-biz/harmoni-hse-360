using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities.Security;

/// <summary>
/// Represents a threat indicator used for threat intelligence
/// </summary>
public class ThreatIndicator : BaseEntity
{
    public string IndicatorType { get; private set; } = string.Empty; // IP, Domain, Hash, Email, etc.
    public string IndicatorValue { get; private set; } = string.Empty;
    public string ThreatType { get; private set; } = string.Empty;
    public int Confidence { get; private set; } // 1-100 scale
    public string Source { get; private set; } = string.Empty;
    
    // Temporal Information
    public DateTime FirstSeen { get; private set; }
    public DateTime LastSeen { get; private set; }
    public bool IsActive { get; private set; }
    
    // Metadata
    public string? Description { get; private set; }
    public string[]? Tags { get; private set; }
    
    // Collections
    private readonly List<SecurityIncident> _relatedIncidents = new();
    public IReadOnlyCollection<SecurityIncident> RelatedIncidents => _relatedIncidents.AsReadOnly();
    
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    
    // Constructor for EF Core
    protected ThreatIndicator() { }
    
    // Factory method
    public static ThreatIndicator Create(
        string indicatorType,
        string indicatorValue,
        string threatType,
        int confidence,
        string source,
        string createdBy,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(indicatorType))
            throw new ArgumentException("Indicator type is required", nameof(indicatorType));
        
        if (string.IsNullOrWhiteSpace(indicatorValue))
            throw new ArgumentException("Indicator value is required", nameof(indicatorValue));
        
        if (string.IsNullOrWhiteSpace(threatType))
            throw new ArgumentException("Threat type is required", nameof(threatType));
        
        if (confidence < 1 || confidence > 100)
            throw new ArgumentOutOfRangeException(nameof(confidence), "Confidence must be between 1 and 100");
        
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source is required", nameof(source));
        
        var now = DateTime.UtcNow;
        
        return new ThreatIndicator
        {
            IndicatorType = indicatorType.ToUpperInvariant(),
            IndicatorValue = indicatorValue,
            ThreatType = threatType,
            Confidence = confidence,
            Source = source,
            FirstSeen = now,
            LastSeen = now,
            IsActive = true,
            Description = description,
            CreatedAt = now,
            CreatedBy = createdBy
        };
    }
    
    // Business Methods
    public void UpdateLastSeen()
    {
        LastSeen = DateTime.UtcNow;
    }
    
    public void UpdateConfidence(int newConfidence, string reason)
    {
        if (newConfidence < 1 || newConfidence > 100)
            throw new ArgumentOutOfRangeException(nameof(newConfidence), "Confidence must be between 1 and 100");
        
        Confidence = newConfidence;
        
        if (!string.IsNullOrWhiteSpace(reason))
        {
            var updateNote = $"\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: Confidence updated to {newConfidence}% - {reason}";
            Description = string.IsNullOrWhiteSpace(Description) ? updateNote.Trim() : Description + updateNote;
        }
    }
    
    public void Deactivate(string reason)
    {
        IsActive = false;
        
        if (!string.IsNullOrWhiteSpace(reason))
        {
            var deactivationNote = $"\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: Deactivated - {reason}";
            Description = string.IsNullOrWhiteSpace(Description) ? deactivationNote.Trim() : Description + deactivationNote;
        }
    }
    
    public void Reactivate(string reason)
    {
        IsActive = true;
        LastSeen = DateTime.UtcNow;
        
        if (!string.IsNullOrWhiteSpace(reason))
        {
            var reactivationNote = $"\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: Reactivated - {reason}";
            Description = string.IsNullOrWhiteSpace(Description) ? reactivationNote.Trim() : Description + reactivationNote;
        }
    }
    
    public void AddTags(params string[] tags)
    {
        if (tags == null || tags.Length == 0)
            return;
        
        var currentTags = Tags?.ToList() ?? new List<string>();
        
        foreach (var tag in tags.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            var cleanTag = tag.Trim().ToLowerInvariant();
            if (!currentTags.Contains(cleanTag))
            {
                currentTags.Add(cleanTag);
            }
        }
        
        Tags = currentTags.ToArray();
    }
    
    public void RemoveTags(params string[] tags)
    {
        if (tags == null || tags.Length == 0 || Tags == null)
            return;
        
        var currentTags = Tags.ToList();
        
        foreach (var tag in tags.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            var cleanTag = tag.Trim().ToLowerInvariant();
            currentTags.Remove(cleanTag);
        }
        
        Tags = currentTags.ToArray();
    }
    
    public void LinkToIncident(SecurityIncident incident)
    {
        if (incident == null)
            throw new ArgumentNullException(nameof(incident));
        
        _relatedIncidents.Add(incident);
        UpdateLastSeen();
    }
    
    // Calculated Properties
    public bool IsHighConfidence => Confidence >= 80;
    
    public bool IsMediumConfidence => Confidence >= 50 && Confidence < 80;
    
    public bool IsLowConfidence => Confidence < 50;
    
    public bool IsRecentlyUpdated => (DateTime.UtcNow - LastSeen).TotalHours <= 24;
    
    public bool IsStale => (DateTime.UtcNow - LastSeen).TotalDays > 30;
    
    public string ConfidenceLevel => Confidence switch
    {
        >= 90 => "Very High",
        >= 80 => "High",
        >= 60 => "Medium",
        >= 40 => "Low",
        _ => "Very Low"
    };
    
    // Validation Methods
    public bool IsValidIndicator()
    {
        return IndicatorType.ToUpperInvariant() switch
        {
            "IP" => IsValidIPAddress(IndicatorValue),
            "DOMAIN" => IsValidDomain(IndicatorValue),
            "EMAIL" => IsValidEmail(IndicatorValue),
            "HASH" => IsValidHash(IndicatorValue),
            "URL" => IsValidUrl(IndicatorValue),
            _ => true // Allow other types for extensibility
        };
    }
    
    // Private validation methods
    private static bool IsValidIPAddress(string ip)
    {
        return System.Net.IPAddress.TryParse(ip, out _);
    }
    
    private static bool IsValidDomain(string domain)
    {
        try
        {
            return Uri.CheckHostName(domain) != UriHostNameType.Unknown;
        }
        catch
        {
            return false;
        }
    }
    
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    private static bool IsValidHash(string hash)
    {
        return hash.Length is 32 or 40 or 64 or 128 && // MD5, SHA1, SHA256, SHA512
               hash.All(c => char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));
    }
    
    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}