using Harmoni360.Application.Features.Statistics.DTOs;
using Harmoni360.Domain.Enums;
using MediatR;

namespace Harmoni360.Application.Features.Statistics.Queries;

public record ExportHsseStatisticsQuery : IRequest<StatisticsFileDto>
{
    public ModuleType? Module { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int HoursWorked { get; init; }
    public int LostTimeInjuries { get; init; }
    public int DaysLost { get; init; }
    public int CompliantRecords { get; init; }
    public int TotalRecords { get; init; }
}
