using Harmoni360.Application.Features.DisposalProviders.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.DisposalProviders.Queries;

public record GetExpiringProvidersQuery(int DaysAhead = 30) : IRequest<List<DisposalProviderDto>>;