namespace Harmoni360.Application.Features.Trainings.DTOs;

public class TrainingCommentDto
{
    public int Id { get; set; }
    public int TrainingId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string CommentType { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int? AuthorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsInternal { get; set; }
    public bool IsEdited { get; set; }
    
    // Threading Support
    public int? ParentCommentId { get; set; }
    public List<TrainingCommentDto> Replies { get; set; } = new();
    public int ReplyCount { get; set; }
    
    // Reactions and Engagement
    public int LikeCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public bool IsPinned { get; set; }
    
    // Visibility and Access
    public bool IsVisible { get; set; }
    public string Visibility { get; set; } = "All"; // All, Instructors, Admins
    
    // Attachments
    public List<CommentAttachmentDto> Attachments { get; set; } = new();
    
    // Mentions and References
    public List<string> MentionedUsers { get; set; } = new();
    public List<int> MentionedUserIds { get; set; } = new();
    
    // Computed Properties
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanReply { get; set; }
    public string CommentTypeIcon => GetCommentTypeIcon(CommentType);
    public string CommentTypeColor => GetCommentTypeColor(CommentType);
    public string RelativeTime => GetRelativeTime(CreatedAt);
    
    private static string GetCommentTypeIcon(string commentType)
    {
        return commentType switch
        {
            "General" => "fa-comment",
            "Feedback" => "fa-comment-dots",
            "Issue" => "fa-exclamation-triangle",
            "Improvement" => "fa-lightbulb",
            "Clarification" => "fa-question-circle",
            "AdminNote" => "fa-user-shield",
            "InstructorNote" => "fa-chalkboard-teacher",
            "ParticipantFeedback" => "fa-user-graduate",
            "AssessmentComment" => "fa-clipboard-check",
            "ComplianceNote" => "fa-balance-scale",
            _ => "fa-comment"
        };
    }
    
    private static string GetCommentTypeColor(string commentType)
    {
        return commentType switch
        {
            "General" => "primary",
            "Feedback" => "info",
            "Issue" => "warning",
            "Improvement" => "success",
            "Clarification" => "secondary",
            "AdminNote" => "danger",
            "InstructorNote" => "warning",
            "ParticipantFeedback" => "info",
            "AssessmentComment" => "primary",
            "ComplianceNote" => "dark",
            _ => "secondary"
        };
    }
    
    private static string GetRelativeTime(DateTime dateTime)
    {
        var timeSpan = DateTime.Now - dateTime;
        
        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
        if (timeSpan.TotalMinutes >= 1)
            return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
        
        return "Just now";
    }
}

public class CommentAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}