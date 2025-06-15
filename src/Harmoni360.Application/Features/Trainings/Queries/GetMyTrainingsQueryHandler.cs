using MediatR;
using Microsoft.EntityFrameworkCore;
using Harmoni360.Application.Common.Interfaces;
using Harmoni360.Application.Common.Models;
using Harmoni360.Application.Features.Trainings.DTOs;

namespace Harmoni360.Application.Features.Trainings.Queries;

public class GetMyTrainingsQueryHandler : IRequestHandler<GetMyTrainingsQuery, PagedList<TrainingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMyTrainingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedList<TrainingDto>> Handle(GetMyTrainingsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Trainings
            .Include(t => t.Participants.Where(p => p.UserId == request.UserId))
            .Where(t => t.Participants.Any(p => p.UserId == request.UserId))
            .AsQueryable();

        // Apply filters
        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        if (request.EnrollmentStatus.HasValue)
        {
            query = query.Where(t => t.Participants.Any(p => 
                p.UserId == request.UserId && 
                p.Status == (Domain.Enums.ParticipantStatus)request.EnrollmentStatus.Value));
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(t => t.ScheduledStartDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(t => t.ScheduledEndDate <= request.ToDate.Value);
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(t => 
                t.Title.Contains(request.SearchTerm) ||
                t.Description.Contains(request.SearchTerm) ||
                t.InstructorName.Contains(request.SearchTerm));
        }

        // Apply sorting
        query = request.SortBy.ToLower() switch
        {
            "title" => request.SortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "status" => request.SortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "type" => request.SortDescending ? query.OrderByDescending(t => t.Type) : query.OrderBy(t => t.Type),
            "createdat" => request.SortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            _ => request.SortDescending ? query.OrderByDescending(t => t.ScheduledStartDate) : query.OrderBy(t => t.ScheduledStartDate)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TrainingDto
            {
                Id = t.Id,
                TrainingCode = t.TrainingCode,
                Title = t.Title,
                Description = t.Description,
                Type = t.Type.ToString(),
                Category = t.Category.ToString(),
                DeliveryMethod = t.DeliveryMethod.ToString(),
                Status = t.Status.ToString(),
                ScheduledStartDate = t.ScheduledStartDate,
                ScheduledEndDate = t.ScheduledEndDate,
                ActualStartDate = t.ActualStartDate,
                ActualEndDate = t.ActualEndDate,
                DurationHours = t.DurationHours,
                Venue = t.Venue,
                VenueAddress = t.VenueAddress,
                MaxParticipants = t.MaxParticipants,
                MinParticipants = t.MinParticipants,
                EnrolledParticipants = t.Participants.Count,
                OnlineLink = t.OnlineLink,
                OnlinePlatform = t.OnlinePlatform,
                LearningObjectives = t.LearningObjectives,
                CourseOutline = t.CourseOutline,
                Prerequisites = t.Prerequisites,
                Materials = t.Materials,
                AssessmentMethod = t.AssessmentMethod.ToString(),
                PassingScore = t.PassingScore,
                InstructorName = t.InstructorName,
                InstructorQualifications = t.InstructorQualifications,
                InstructorContact = t.InstructorContact,
                IsExternalInstructor = t.IsExternalInstructor,
                IssuesCertificate = t.IssuesCertificate,
                CertificationType = t.CertificationType.ToString(),
                CertificateValidityPeriod = t.CertificateValidityPeriod.ToString(),
                CertifyingBody = t.CertifyingBody,
                CostPerParticipant = t.CostPerParticipant,
                TotalBudget = t.TotalBudget,
                Currency = t.Currency,
                IsK3MandatoryTraining = t.IsK3MandatoryTraining,
                K3RegulationReference = t.K3RegulationReference,
                IsBPJSCompliant = t.IsBPJSCompliant,
                MinistryApprovalNumber = t.MinistryApprovalNumber,
                RequiresGovernmentCertification = t.RequiresGovernmentCertification,
                IndonesianTrainingStandard = t.IndonesianTrainingStandard,
                AverageRating = t.AverageRating,
                TotalRatings = t.TotalRatings,
                EvaluationSummary = t.EvaluationSummary,
                ImprovementActions = t.ImprovementActions,
                CreatedAt = t.CreatedAt,
                CreatedBy = t.CreatedBy,
                LastModifiedAt = t.LastModifiedAt,
                LastModifiedBy = t.LastModifiedBy
            })
            .ToListAsync(cancellationToken);

        return new PagedList<TrainingDto>(items, totalCount, request.Page, request.PageSize);
    }
}