using MediatR;

namespace Harmoni360.Application.Features.DisposalProviders.Commands;

public record DeleteDisposalProviderCommand(int Id) : IRequest<Unit>;