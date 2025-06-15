using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Features.Trainings.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;
using Harmoni360.Domain.ValueObjects;

namespace Harmoni360.Application.Features.Trainings.Commands;

public class CreateTrainingCommandHandler : IRequestHandler<CreateTrainingCommand, TrainingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateTrainingCommandHandler> _logger;

    public CreateTrainingCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<CreateTrainingCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<TrainingDto> Handle(CreateTrainingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user details
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            // Create GeoLocation if coordinates provided
            GeoLocation? geoLocation = null;
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                geoLocation = GeoLocation.Create(
                    request.Latitude.Value,
                    request.Longitude.Value);
            }

            // Create the training
            var training = Training.Create(
                request.Title,
                request.Description,
                request.Type,
                request.Category,
                request.DeliveryMethod,
                request.ScheduledStartDate,
                request.ScheduledEndDate,
                request.Venue,
                request.MaxParticipants,
                request.MinParticipants,
                request.InstructorName,
                request.CostPerParticipant,
                geoLocation);

            // Set additional details
            training.GetType().GetProperty(nameof(Training.VenueAddress))?.SetValue(training, request.VenueAddress);
            training.GetType().GetProperty(nameof(Training.OnlineLink))?.SetValue(training, request.OnlineLink);
            training.GetType().GetProperty(nameof(Training.OnlinePlatform))?.SetValue(training, request.OnlinePlatform);
            training.GetType().GetProperty(nameof(Training.LearningObjectives))?.SetValue(training, request.LearningObjectives);
            training.GetType().GetProperty(nameof(Training.CourseOutline))?.SetValue(training, request.CourseOutline);
            training.GetType().GetProperty(nameof(Training.Prerequisites))?.SetValue(training, request.Prerequisites);
            training.GetType().GetProperty(nameof(Training.Materials))?.SetValue(training, request.Materials);

            // Set instructor details
            training.SetInstructorDetails(
                request.InstructorName,
                request.InstructorQualifications,
                request.InstructorContact,
                request.IsExternalInstructor);

            // Set certification details
            training.SetCertificationDetails(
                request.IssuesCertificate,
                request.CertificationType,
                request.CertificateValidityPeriod,
                request.CertifyingBody);

            // Set assessment details
            training.SetAssessmentDetails(
                request.AssessmentMethod,
                request.PassingScore);

            // Set Indonesian compliance
            if (request.IsK3MandatoryTraining || !string.IsNullOrEmpty(request.K3RegulationReference))
            {
                training.SetIndonesianCompliance(
                    request.IsK3MandatoryTraining,
                    request.K3RegulationReference,
                    request.IsBPJSCompliant,
                    request.MinistryApprovalNumber,
                    request.IndonesianTrainingStandard);
            }

            // Set audit fields
            training.CreatedBy = currentUser.Name;
            training.CreatedAt = DateTime.UtcNow;

            _context.Trainings.Add(training);
            await _context.SaveChangesAsync(cancellationToken);

            // Add requirements if provided
            foreach (var requirementRequest in request.Requirements)
            {
                training.AddRequirement(
                    requirementRequest.RequirementDescription,
                    requirementRequest.IsMandatory,
                    requirementRequest.DueDate);
            }

            // Save requirements
            if (request.Requirements.Any())
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Training created successfully. ID: {TrainingId}, Code: {TrainingCode}", 
                training.Id, training.TrainingCode);

            // Return DTO
            return await MapToDto(training);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating training for user {UserId}", _currentUserService.UserId);
            throw;
        }
    }

    private async Task<TrainingDto> MapToDto(Training training)
    {
        // Load related data
        var dbContext = (DbContext)_context;
        await dbContext.Entry(training)
            .Collection(t => t.Participants)
            .LoadAsync();
        await dbContext.Entry(training)
            .Collection(t => t.Requirements)
            .LoadAsync();
        await dbContext.Entry(training)
            .Collection(t => t.Attachments)
            .LoadAsync();
        await dbContext.Entry(training)
            .Collection(t => t.Comments)
            .LoadAsync();
        await dbContext.Entry(training)
            .Collection(t => t.Certifications)
            .LoadAsync();

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