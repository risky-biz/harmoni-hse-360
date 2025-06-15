using Harmoni360.Domain.Common;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Domain.Entities.Waste;

public class WasteComment : BaseEntity, IAuditableEntity
{
    public int WasteReportId { get; private set; }
    public WasteReport WasteReport { get; private set; } = null!;
    public string Comment { get; private set; } = string.Empty;
    public CommentType Type { get; private set; }
    public int CommentedById { get; private set; }
    public User CommentedBy { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected WasteComment() { }

    public static WasteComment Create(int wasteReportId, int commentedById, string comment, CommentType type, string createdBy)
    {
        return new WasteComment
        {
            WasteReportId = wasteReportId,
            CommentedById = commentedById,
            Comment = comment,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }
}
