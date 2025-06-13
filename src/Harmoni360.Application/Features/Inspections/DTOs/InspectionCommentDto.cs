namespace Harmoni360.Application.Features.Inspections.DTOs;

public class InspectionCommentDto
{
    public int Id { get; set; }
    public int InspectionId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public int? ParentCommentId { get; set; }
    public bool IsReply { get; set; }
    public bool HasReplies { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? LastModifiedBy { get; set; }

    public List<InspectionCommentDto> Replies { get; set; } = new();
}