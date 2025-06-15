using MediatR;
using Harmoni360.Application.Features.RiskAssessments.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Harmoni360.Application.Features.RiskAssessments.Commands;

public record ApproveRiskAssessmentCommand : IRequest<RiskAssessmentDto>
{
    [Required]
    public int Id { get; init; }
    
    [MaxLength(1000)]
    public string? ApprovalNotes { get; init; }
}