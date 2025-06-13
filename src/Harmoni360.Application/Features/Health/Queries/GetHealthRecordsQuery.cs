using MediatR;
using Harmoni360.Domain.Entities;
using Harmoni360.Application.Features.Health.DTOs;

namespace Harmoni360.Application.Features.Health.Queries;

public record GetHealthRecordsQuery : IRequest<GetHealthRecordsResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public PersonType? PersonType { get; init; }
    public string? Department { get; init; }
    public bool? HasCriticalConditions { get; init; }
    public bool? HasExpiringVaccinations { get; init; }
    public bool IncludeInactive { get; init; } = false;
    public string? SortBy { get; init; } = "CreatedAt";
    public bool SortDescending { get; init; } = true;
}

public class GetHealthRecordsResponse
{
    public List<HealthRecordDto> Records { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    // Filter summary
    public int ActiveRecords { get; set; }
    public int StudentRecords { get; set; }
    public int StaffRecords { get; set; }
    public int CriticalConditionsCount { get; set; }
    public int ExpiringVaccinationsCount { get; set; }
}