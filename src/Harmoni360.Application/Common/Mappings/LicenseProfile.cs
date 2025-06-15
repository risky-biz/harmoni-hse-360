using AutoMapper;
using Harmoni360.Application.Features.Licenses.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Common.Mappings;

public class LicenseProfile : Profile
{
    public LicenseProfile()
    {
        CreateMap<License, LicenseDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.TypeDisplay, opt => opt.MapFrom(src => GetLicenseTypeDisplay(src.Type)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => GetLicenseStatusDisplay(src.Status)))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.PriorityDisplay, opt => opt.MapFrom(src => GetLicensePriorityDisplay(src.Priority)))
            .ForMember(dest => dest.RiskLevel, opt => opt.MapFrom(src => src.RiskLevel.ToString()))
            .ForMember(dest => dest.RiskLevelDisplay, opt => opt.MapFrom(src => GetRiskLevelDisplay(src.RiskLevel)))
            .ForMember(dest => dest.ConditionsText, opt => opt.MapFrom(src => src.Conditions))
            .ForMember(dest => dest.DaysUntilExpiry, opt => opt.MapFrom(src => src.DaysUntilExpiry))
            .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.IsExpired))
            .ForMember(dest => dest.IsExpiringSoon, opt => opt.MapFrom(src => src.IsExpiringSoon))
            .ForMember(dest => dest.RequiresRenewal, opt => opt.MapFrom(src => src.RequiresRenewal))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ForMember(dest => dest.Renewals, opt => opt.MapFrom(src => src.Renewals))
            .ForMember(dest => dest.Conditions, opt => opt.MapFrom(src => src.LicenseConditions))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.LastModifiedAt))
            .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.LastModifiedBy));

        CreateMap<LicenseAttachment, LicenseAttachmentDto>()
            .ForMember(dest => dest.AttachmentType, opt => opt.MapFrom(src => src.AttachmentType.ToString()))
            .ForMember(dest => dest.AttachmentTypeDisplay, opt => opt.MapFrom(src => GetAttachmentTypeDisplay(src.AttachmentType)));

        CreateMap<LicenseRenewal, LicenseRenewalDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => GetRenewalStatusDisplay(src.Status)));

        CreateMap<LicenseCondition, LicenseConditionDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => GetConditionStatusDisplay(src.Status)));

        CreateMap<LicenseAuditLog, LicenseAuditLogDto>()
            .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action.ToString()))
            .ForMember(dest => dest.ActionDescription, opt => opt.MapFrom(src => src.ActionDescription))
            .ForMember(dest => dest.PerformedBy, opt => opt.MapFrom(src => src.PerformedBy))
            .ForMember(dest => dest.PerformedAt, opt => opt.MapFrom(src => src.PerformedAt));
    }

    private static string GetLicenseTypeDisplay(LicenseType type) => type switch
    {
        LicenseType.Environmental => "Environmental License",
        LicenseType.Safety => "Safety License",
        LicenseType.Health => "Health License",
        LicenseType.Construction => "Construction License",
        LicenseType.Operating => "Operating License",
        LicenseType.Transport => "Transport License",
        LicenseType.Waste => "Waste Management License",
        LicenseType.Chemical => "Chemical License",
        LicenseType.Radiation => "Radiation License",
        LicenseType.Fire => "Fire Safety License",
        LicenseType.Electrical => "Electrical License",
        LicenseType.Mechanical => "Mechanical License",
        LicenseType.Professional => "Professional License",
        LicenseType.Business => "Business License",
        LicenseType.Import => "Import License",
        LicenseType.Export => "Export License",
        LicenseType.Other => "Other License",
        _ => type.ToString()
    };

    private static string GetLicenseStatusDisplay(LicenseStatus status) => status switch
    {
        LicenseStatus.Draft => "Draft",
        LicenseStatus.PendingSubmission => "Pending Submission",
        LicenseStatus.Submitted => "Submitted",
        LicenseStatus.UnderReview => "Under Review",
        LicenseStatus.Approved => "Approved",
        LicenseStatus.Active => "Active",
        LicenseStatus.Rejected => "Rejected",
        LicenseStatus.Expired => "Expired",
        LicenseStatus.Suspended => "Suspended",
        LicenseStatus.Revoked => "Revoked",
        LicenseStatus.PendingRenewal => "Pending Renewal",
        _ => status.ToString()
    };

    private static string GetLicensePriorityDisplay(LicensePriority priority) => priority switch
    {
        LicensePriority.Low => "Low Priority",
        LicensePriority.Medium => "Medium Priority",
        LicensePriority.High => "High Priority",
        LicensePriority.Critical => "Critical Priority",
        _ => priority.ToString()
    };

    private static string GetRiskLevelDisplay(RiskLevel riskLevel) => riskLevel switch
    {
        RiskLevel.Low => "Low Risk",
        RiskLevel.Medium => "Medium Risk",
        RiskLevel.High => "High Risk",
        RiskLevel.Critical => "Critical Risk",
        _ => riskLevel.ToString()
    };

    private static string GetAttachmentTypeDisplay(LicenseAttachmentType type) => type switch
    {
        LicenseAttachmentType.Application => "Application Document",
        LicenseAttachmentType.SupportingDocument => "Supporting Document",
        LicenseAttachmentType.Certificate => "Certificate",
        LicenseAttachmentType.Compliance => "Compliance Document",
        LicenseAttachmentType.Insurance => "Insurance Document",
        LicenseAttachmentType.TechnicalSpec => "Technical Specification",
        LicenseAttachmentType.LegalDocument => "Legal Document",
        LicenseAttachmentType.RenewalDocument => "Renewal Document",
        LicenseAttachmentType.InspectionReport => "Inspection Report",
        LicenseAttachmentType.Other => "Other Document",
        _ => type.ToString()
    };

    private static string GetRenewalStatusDisplay(LicenseRenewalStatus status) => status switch
    {
        LicenseRenewalStatus.Draft => "Draft",
        LicenseRenewalStatus.Submitted => "Submitted",
        LicenseRenewalStatus.UnderReview => "Under Review",
        LicenseRenewalStatus.Approved => "Approved",
        LicenseRenewalStatus.Rejected => "Rejected",
        LicenseRenewalStatus.Expired => "Expired",
        _ => status.ToString()
    };

    private static string GetConditionStatusDisplay(LicenseConditionStatus status) => status switch
    {
        LicenseConditionStatus.Pending => "Pending",
        LicenseConditionStatus.InProgress => "In Progress",
        LicenseConditionStatus.Completed => "Completed",
        LicenseConditionStatus.Overdue => "Overdue",
        LicenseConditionStatus.Waived => "Waived",
        _ => status.ToString()
    };
}