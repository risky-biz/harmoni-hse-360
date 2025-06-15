using MediatR;
using Microsoft.AspNetCore.Http;
using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Trainings.Commands;

public record UploadTrainingAttachmentCommand : IRequest<TrainingAttachmentDto>
{
    public int TrainingId { get; init; }
    public IFormFile File { get; init; } = null!;
    public TrainingAttachmentType AttachmentType { get; init; }
    public string Description { get; init; } = string.Empty;
    public bool IsPublic { get; init; } = false;
    public bool IsInstructorOnly { get; init; } = false;
    public bool IsComplianceDocument { get; init; } = false;
    public string RegulatoryReference { get; init; } = string.Empty;
    public bool IsK3Document { get; init; } = false;
    public string K3DocumentType { get; init; } = string.Empty;
    public bool RequiresApproval { get; init; } = false;
}