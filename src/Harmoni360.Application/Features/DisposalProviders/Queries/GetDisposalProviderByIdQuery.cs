using Harmoni360.Application.Features.DisposalProviders.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.DisposalProviders.Queries;

public record GetDisposalProviderByIdQuery(int Id) : IRequest<DisposalProviderDto?>;