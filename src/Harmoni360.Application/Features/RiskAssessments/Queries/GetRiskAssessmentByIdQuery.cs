using MediatR;
using Harmoni360.Application.Features.RiskAssessments.DTOs;

namespace Harmoni360.Application.Features.RiskAssessments.Queries;

public record GetRiskAssessmentByIdQuery(int Id) : IRequest<RiskAssessmentDetailDto?>;