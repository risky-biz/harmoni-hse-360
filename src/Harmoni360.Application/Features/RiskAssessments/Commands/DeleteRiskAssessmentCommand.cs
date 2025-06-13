using MediatR;

namespace Harmoni360.Application.Features.RiskAssessments.Commands;

public record DeleteRiskAssessmentCommand(int Id) : IRequest;