using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Trainings.DTOs;

namespace Harmoni360.Application.Features.Trainings.Queries;

public class GetTrainingByIdQueryHandler : IRequestHandler<GetTrainingByIdQuery, TrainingDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetTrainingByIdQueryHandler> _logger;

    public GetTrainingByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<GetTrainingByIdQueryHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<TrainingDto?> Handle(GetTrainingByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Trainings.AsQueryable();

            // Include related data based on request
            if (request.IncludeParticipants)
                query = query.Include(t => t.Participants);
            
            if (request.IncludeRequirements)
                query = query.Include(t => t.Requirements);
            
            if (request.IncludeAttachments)
                query = query.Include(t => t.Attachments);
            
            if (request.IncludeComments)
                query = query.Include(t => t.Comments);
            
            if (request.IncludeCertifications)
                query = query.Include(t => t.Certifications);

            var training = await query
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (training == null)
            {
                _logger.LogWarning("Training with ID {TrainingId} not found", request.Id);
                return null;
            }

            _logger.LogInformation("Retrieved training {TrainingCode} for user {UserId}", 
                training.TrainingCode, _currentUserService.UserId);

            return MapToDto(training);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving training {TrainingId} for user {UserId}", 
                request.Id, _currentUserService.UserId);
            throw;
        }
    }

    private static TrainingDto MapToDto(Domain.Entities.Training training)
    {
        return new TrainingDto
        {
            Id = training.Id,
            TrainingCode = training.TrainingCode,
            Title = training.Title,
            Description = training.Description,
            Type = training.Type.ToString(),
            Category = training.Category.ToString(),
            Status = training.Status.ToString(),
            Priority = training.Priority.ToString(),
            DeliveryMethod = training.DeliveryMethod.ToString(),
            ScheduledStartDate = training.ScheduledStartDate,
            ScheduledEndDate = training.ScheduledEndDate,
            ActualStartDate = training.ActualStartDate,
            ActualEndDate = training.ActualEndDate,
            DurationHours = training.DurationHours,
            MaxParticipants = training.MaxParticipants,
            MinParticipants = training.MinParticipants,
            EnrolledParticipants = training.Participants.Count,
            Venue = training.Venue,
            VenueAddress = training.VenueAddress,
            GeoLocation = training.GeoLocation != null ? new GeoLocationDto
            {
                Latitude = training.GeoLocation.Latitude,
                Longitude = training.GeoLocation.Longitude,
                Address = training.VenueAddress,
                LocationDescription = training.Venue
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
            LastModifiedBy = training.LastModifiedBy,
            Participants = training.Participants.Select(p => new TrainingParticipantDto
            {
                Id = p.Id,
                UserId = p.UserId,
                UserName = p.UserName,
                UserDepartment = p.UserDepartment,
                UserPosition = p.UserPosition,
                Status = p.Status.ToString(),
                EnrolledAt = p.EnrolledAt,
                EnrolledBy = p.EnrolledBy,
                AttendanceMarked = p.AttendanceStartTime.HasValue,
                AttendanceDate = p.AttendanceStartTime,
                FinalScore = p.FinalScore,
                Passed = p.HasPassed,
                CompletedAt = p.CompletedAt,
                CertificateIssued = p.CertificateIssuedAt.HasValue,
                CertificateIssuedAt = p.CertificateIssuedAt,
                Feedback = p.TrainingFeedback
            }).ToList(),
            Requirements = training.Requirements.Select(r => new TrainingRequirementDto
            {
                Id = r.Id,
                RequirementDescription = r.RequirementDescription,
                IsMandatory = r.IsMandatory,
                Status = r.Status.ToString(),
                DueDate = r.DueDate,
                CompletedAt = r.CompletedDate,
                CompletedBy = r.CompletedBy,
                CompetencyLevel = "", // Not available in entity
                VerificationMethod = r.VerificationMethod,
                IsVerified = r.IsVerified,
                VerifiedAt = r.VerificationDate,
                VerifiedBy = r.VerifiedBy
            }).ToList(),
            Attachments = training.Attachments.Select(a => new TrainingAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                OriginalFileName = a.OriginalFileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                UploadedBy = a.UploadedBy,
                UploadedAt = a.UploadedAt,
                AttachmentType = a.AttachmentType.ToString(),
                Description = a.Description,
                IsTrainingMaterial = a.AttachmentType == Domain.Enums.TrainingAttachmentType.HandoutMaterial || a.AttachmentType == Domain.Enums.TrainingAttachmentType.CourseOutline,
                IsPublic = a.IsPublic
            }).ToList(),
            Comments = training.Comments.Select(c => new TrainingCommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CommentType = c.CommentType.ToString(),
                AuthorName = c.AuthorName,
                AuthorId = c.AuthorId,
                CreatedAt = c.CreatedAt,
                IsInternal = c.IsInstructorOnly || c.IsPrivateNote,
                ParentCommentId = c.ParentCommentId
            }).ToList(),
            Certifications = training.Certifications.Select(cert => new TrainingCertificationDto
            {
                Id = cert.Id,
                CertificateNumber = cert.CertificateNumber,
                ParticipantUserId = cert.UserId,
                ParticipantName = cert.UserName,
                CertificationType = cert.CertificationType.ToString(),
                IssuedDate = cert.IssuedDate,
                ExpiryDate = cert.ValidUntil,
                CertifyingBody = cert.CertifyingBody,
                Status = cert.GetValidityStatus(),
                Score = cert.FinalScore,
                DigitalCertificateUrl = cert.VerificationUrl,
                VerificationCode = cert.QRCodeData
            }).ToList()
        };
    }
}