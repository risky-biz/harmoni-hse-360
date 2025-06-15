using Harmoni360.Application.Features.Inspections.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.Inspections.Queries;

public record GetInspectionByIdQuery(int InspectionId) : IRequest<InspectionDetailDto>;