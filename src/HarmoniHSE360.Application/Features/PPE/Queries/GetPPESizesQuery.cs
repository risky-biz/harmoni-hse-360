using HarmoniHSE360.Application.Features.PPE.DTOs;
using MediatR;

namespace HarmoniHSE360.Application.Features.PPE.Queries;

public class GetPPESizesQuery : IRequest<List<PPESizeDto>>
{
    public bool? IsActive { get; set; }
    public string? SearchTerm { get; set; }
}