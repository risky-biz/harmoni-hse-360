using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Domain.Entities;

namespace Harmoni360.Application.Features.Trainings.Commands;

public class UpdateTrainingCommandHandler : IRequestHandler<UpdateTrainingCommand, TrainingDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateTrainingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TrainingDto> Handle(UpdateTrainingCommand request, CancellationToken cancellationToken)
    {
        var training = await _context.Trainings
            .Include(t => t.Participants)
            .Include(t => t.Requirements)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.Certifications)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (training == null)
        {
            throw new InvalidOperationException($"Training with ID {request.Id} not found.");
        }

        training.SetInstructorDetails(
            request.InstructorName,
            request.InstructorQualifications,
            request.InstructorContact,
            request.IsExternalInstructor);

        training.SetCertificationDetails(
            request.IssuesCertificate,
            request.CertificationType,
            request.CertificateValidityPeriod,
            request.CertifyingBody);

        training.SetIndonesianCompliance(
            request.IsK3MandatoryTraining,
            request.K3RegulationReference,
            request.IsBPJSCompliant,
            request.MinistryApprovalNumber,
            request.IndonesianTrainingStandard);

        await _context.SaveChangesAsync(cancellationToken);

        return new TrainingDto
        {
            Id = training.Id,
            TrainingCode = training.TrainingCode,
            Title = training.Title,
            Description = training.Description,
            Type = training.Type.ToString(),
            Category = training.Category.ToString(),
            DeliveryMethod = training.DeliveryMethod.ToString(),
            Status = training.Status.ToString(),
            ScheduledStartDate = training.ScheduledStartDate,
            ScheduledEndDate = training.ScheduledEndDate,
            ActualStartDate = training.ActualStartDate,
            ActualEndDate = training.ActualEndDate,
            DurationHours = training.DurationHours,
            Venue = training.Venue,
            VenueAddress = training.VenueAddress,
            MaxParticipants = training.MaxParticipants,
            MinParticipants = training.MinParticipants,
            EnrolledParticipants = training.Participants.Count,
            GeoLocation = training.GeoLocation != null ? new GeoLocationDto 
            { 
                Latitude = training.GeoLocation.Latitude, 
                Longitude = training.GeoLocation.Longitude,
                Address = training.VenueAddress
            } : null,
            OnlineLink = training.OnlineLink,
            OnlinePlatform = training.OnlinePlatform,
            LearningObjectives = training.LearningObjectives,
            CourseOutline = training.CourseOutline,
            Prerequisites = training.Prerequisites,
            Materials = training.Materials,
            AssessmentMethod = training.AssessmentMethod.ToString(),
            PassingScore = training.PassingScore,
            InstructorName = training.InstructorName,
            InstructorQualifications = training.InstructorQualifications,
            InstructorContact = training.InstructorContact,
            IsExternalInstructor = training.IsExternalInstructor,
            IssuesCertificate = training.IssuesCertificate,
            CertificationType = training.CertificationType.ToString(),
            CertificateValidityPeriod = training.CertificateValidityPeriod.ToString(),
            CertifyingBody = training.CertifyingBody,
            CostPerParticipant = training.CostPerParticipant,
            TotalBudget = training.TotalBudget,
            Currency = training.Currency,
            IsK3MandatoryTraining = training.IsK3MandatoryTraining,
            K3RegulationReference = training.K3RegulationReference,
            IsBPJSCompliant = training.IsBPJSCompliant,
            MinistryApprovalNumber = training.MinistryApprovalNumber,
            RequiresGovernmentCertification = training.RequiresGovernmentCertification,
            IndonesianTrainingStandard = training.IndonesianTrainingStandard,
            AverageRating = training.AverageRating,
            TotalRatings = training.TotalRatings,
            EvaluationSummary = training.EvaluationSummary,
            ImprovementActions = training.ImprovementActions,
            CreatedAt = training.CreatedAt,
            CreatedBy = training.CreatedBy,
            LastModifiedAt = training.LastModifiedAt,
            LastModifiedBy = training.LastModifiedBy
        };
    }
}