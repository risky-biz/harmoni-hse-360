using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Trainings.Commands;

public class EnrollParticipantCommandHandler : IRequestHandler<EnrollParticipantCommand, TrainingParticipantDto>
{
    private readonly IApplicationDbContext _context;

    public EnrollParticipantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TrainingParticipantDto> Handle(EnrollParticipantCommand request, CancellationToken cancellationToken)
    {
        var training = await _context.Trainings
            .Include(t => t.Participants)
            .FirstOrDefaultAsync(t => t.Id == request.TrainingId, cancellationToken);

        if (training == null)
        {
            throw new InvalidOperationException($"Training with ID {request.TrainingId} not found.");
        }

        // Check if participant is already enrolled
        var existingParticipant = training.Participants.FirstOrDefault(p => p.UserId == request.UserId);
        if (existingParticipant != null)
        {
            throw new InvalidOperationException("User is already enrolled in this training.");
        }

        // Check capacity
        if (training.Participants.Count >= training.MaxParticipants)
        {
            throw new InvalidOperationException("Training has reached maximum capacity.");
        }

        var participant = TrainingParticipant.Create(
            request.TrainingId,
            request.UserId,
            request.UserName,
            request.Department,
            request.Position,
            "System", // enrolledBy
            request.UserEmail,
            request.UserPhone,
            request.EmployeeId);

        // Note: AddEnrollmentNotes method doesn't exist in the domain entity
        // The notes would typically be handled through a separate mechanism

        _context.TrainingParticipants.Add(participant);
        await _context.SaveChangesAsync(cancellationToken);

        return new TrainingParticipantDto
        {
            Id = participant.Id,
            TrainingId = participant.TrainingId,
            UserId = participant.UserId,
            UserName = participant.UserName,
            UserDepartment = participant.UserDepartment,
            UserPosition = participant.UserPosition,
            Status = participant.Status.ToString(),
            EnrolledAt = participant.EnrolledAt,
            EnrolledBy = participant.EnrolledBy
        };
    }
}