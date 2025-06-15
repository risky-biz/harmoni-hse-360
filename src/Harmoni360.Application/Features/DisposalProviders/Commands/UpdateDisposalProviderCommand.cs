using Harmoni360.Application.Features.DisposalProviders.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.DisposalProviders.Commands;

public record UpdateDisposalProviderCommand(int Id, string Name, string LicenseNumber, DateTime LicenseExpiryDate) : IRequest<DisposalProviderDto>;