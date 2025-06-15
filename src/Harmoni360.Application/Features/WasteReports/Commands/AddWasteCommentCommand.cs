using MediatR;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.WasteReports.Commands;

public record AddWasteCommentCommand(
    int WasteReportId,
    string Comment,
    CommentType Type,
    int CommentedById) : IRequest<WasteCommentDto>;

public class WasteCommentDto
{
    public int Id { get; set; }
    public int WasteReportId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int CommentedById { get; set; }
    public string CommentedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}