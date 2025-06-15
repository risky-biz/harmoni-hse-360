using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities.Inspections;

public class InspectionComment : BaseEntity, IAuditableEntity
{
    public int InspectionId { get; private set; }
    public int UserId { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public bool IsInternal { get; private set; }
    public int? ParentCommentId { get; private set; }

    // Navigation Properties
    public virtual Inspection Inspection { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;
    public virtual InspectionComment? ParentComment { get; private set; }
    public virtual ICollection<InspectionComment> Replies { get; private set; } = new List<InspectionComment>();

    // IAuditableEntity
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    private InspectionComment() { }

    public static InspectionComment Create(
        int inspectionId,
        int userId,
        string comment,
        bool isInternal = false,
        int? parentCommentId = null)
    {
        return new InspectionComment
        {
            InspectionId = inspectionId,
            UserId = userId,
            Comment = comment,
            IsInternal = isInternal,
            ParentCommentId = parentCommentId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateComment(string comment)
    {
        Comment = comment;
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool IsReply => ParentCommentId.HasValue;
    public bool HasReplies => Replies.Any();
}