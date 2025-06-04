using HarmoniHSE360.Application.Features.Incidents.DTOs;
using MediatR;

namespace HarmoniHSE360.Application.Features.Incidents.Queries;

public class GetIncidentDetailQuery : IRequest<IncidentDetailDto?>
{
    public int Id { get; set; }
}