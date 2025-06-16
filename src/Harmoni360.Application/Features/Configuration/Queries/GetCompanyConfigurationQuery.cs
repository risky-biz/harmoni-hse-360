using Harmoni360.Application.Features.Configuration.DTOs;
using MediatR;

namespace Harmoni360.Application.Features.Configuration.Queries;

public record GetCompanyConfigurationQuery : IRequest<CompanyConfigurationDto?>;

public record GetActiveCompanyConfigurationQuery : IRequest<CompanyConfigurationDto?>;