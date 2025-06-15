using Harmoni360.Application.Features.DisposalProviders.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.DisposalProviders.Commands;

public record CreateDisposalProviderCommand(string Name, string LicenseNumber, DateTime LicenseExpiryDate) : IRequest<DisposalProviderDto>;
