using Harmoni360.Application.Features.DisposalProviders.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.DisposalProviders.Queries;

public record SearchDisposalProvidersQuery(
    string? SearchTerm = null,
    ProviderStatus? Status = null,
    bool? IncludeInactive = false,
    bool? OnlyExpiring = false,
    int? ExpiringDays = 30
) : IRequest<List<DisposalProviderDto>>;