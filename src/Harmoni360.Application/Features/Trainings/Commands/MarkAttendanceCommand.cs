using MediatR;
using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.Trainings.Commands;

public record MarkAttendanceCommand : IRequest<TrainingParticipantDto>
{
    public int TrainingId { get; init; }
    public int ParticipantId { get; init; }
    public AttendanceStatus AttendanceStatus { get; init; }
    public DateTime AttendanceDate { get; init; }
    public string AttendanceNotes { get; init; } = string.Empty;
    public string MarkedBy { get; init; } = string.Empty;
}