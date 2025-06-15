using Harmoni360.Domain.Common;

namespace Harmoni360.Domain.Entities.Audits;

public class AuditComment : BaseEntity, IAuditableEntity
{
    public int AuditId { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public string CommentedBy { get; private set; } = string.Empty;
    public DateTime CommentedAt { get; private set; }
    public int? AuditItemId { get; private set; }
    public int? AuditFindingId { get; private set; }
    public string? Category { get; private set; }
    public bool IsInternal { get; private set; }

    // Navigation Properties
    public virtual Audit Audit { get; private set; } = null!;
    public virtual AuditItem? AuditItem { get; private set; }
    public virtual AuditFinding? AuditFinding { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    private AuditComment() { }

    public static AuditComment Create(
        int auditId,
        string comment,
        string commentedBy,
        int? auditItemId = null,
        int? auditFindingId = null,
        string? category = null,
        bool isInternal = false)
    {
        return new AuditComment
        {
            AuditId = auditId,
            Comment = comment,
            CommentedBy = commentedBy,
            CommentedAt = DateTime.UtcNow,
            AuditItemId = auditItemId,
            AuditFindingId = auditFindingId,
            Category = category,
            IsInternal = isInternal,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateComment(string comment)
    {
        Comment = comment;
        LastModifiedAt = DateTime.UtcNow;
    }
}