using Harmoni360.Application.Features.DisposalProviders.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.DisposalProviders.Commands;

public record ChangeProviderStatusCommand(int Id, ProviderStatus Status) : IRequest<DisposalProviderDto>;