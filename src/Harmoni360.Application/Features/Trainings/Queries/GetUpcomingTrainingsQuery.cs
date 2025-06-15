using MediatR;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Trainings.Queries;

public record GetUpcomingTrainingsQuery : IRequest<PagedList<TrainingDto>>
{
    public int? UserId { get; init; }
    public TrainingType? Type { get; init; }
    public TrainingCategory? Category { get; init; }
    public int DaysAhead { get; init; } = 30;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string SearchTerm { get; init; } = string.Empty;
    public string SortBy { get; init; } = "ScheduledStartDate";
    public bool SortDescending { get; init; } = false;
}