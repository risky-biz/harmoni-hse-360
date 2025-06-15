using MediatR;
using Harmoni360.Application.Features.Trainings.DTOs;

namespace Harmoni360.Application.Features.Trainings.Commands;

public record EnrollParticipantCommand : IRequest<TrainingParticipantDto>
{
    public int TrainingId { get; init; }
    public int UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string UserPhone { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public string EmployeeId { get; init; } = string.Empty;
    public bool IsMandatory { get; init; }
    public string EnrollmentNotes { get; init; } = string.Empty;
}