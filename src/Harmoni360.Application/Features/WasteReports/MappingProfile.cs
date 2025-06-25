using AutoMapper;
using Harmoni360.Application.Features.WasteReports.DTOs;
using Harmoni360.Domain.Entities;
using Harmoni360.Domain.Entities.Waste;
using Harmoni360.Domain.Enums;

namespace Harmoni360.Application.Features.WasteReports
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // WasteReport entity to DTO mappings
            CreateMap<WasteReport, WasteReportDto>()
                .ForMember(dest => dest.Classification, opt => opt.MapFrom(src => MapCategoryToClassification(src.Category)))
                .ForMember(dest => dest.ClassificationDisplay, opt => opt.MapFrom(src => MapCategoryToString(src.Category)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MapDisposalStatusToReportStatus(src.DisposalStatus)))
                .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => MapDisposalStatusToString(src.DisposalStatus)))
                .ForMember(dest => dest.ReportDate, opt => opt.MapFrom(src => src.GeneratedDate))
                .ForMember(dest => dest.ReportedBy, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.Name : "Unknown"))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.LastModifiedAt))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.LastModifiedBy))
                .ForMember(dest => dest.EstimatedQuantity, opt => opt.Ignore())
                .ForMember(dest => dest.QuantityUnit, opt => opt.Ignore())
                .ForMember(dest => dest.DisposalMethod, opt => opt.Ignore())
                .ForMember(dest => dest.DisposalDate, opt => opt.Ignore())
                .ForMember(dest => dest.DisposedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DisposalCost, opt => opt.Ignore())
                .ForMember(dest => dest.ContractorName, opt => opt.Ignore())
                .ForMember(dest => dest.ManifestNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Treatment, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.Ignore())
                .ForMember(dest => dest.Comments, opt => opt.Ignore());

            CreateMap<WasteReport, WasteReportSummaryDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => MapCategoryToString(src.Category)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MapDisposalStatusToString(src.DisposalStatus)))
                .ForMember(dest => dest.ReportDate, opt => opt.MapFrom(src => src.GeneratedDate))
                .ForMember(dest => dest.ReportedBy, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.Name : "Unknown"))
                .ForMember(dest => dest.EstimatedQuantity, opt => opt.Ignore())
                .ForMember(dest => dest.QuantityUnit, opt => opt.Ignore())
                .ForMember(dest => dest.DisposalDate, opt => opt.Ignore())
                .ForMember(dest => dest.DisposalCost, opt => opt.Ignore())
                .ForMember(dest => dest.CommentsCount, opt => opt.Ignore())
                .ForMember(dest => dest.IsOverdue, opt => opt.Ignore())
                .ForMember(dest => dest.CanEdit, opt => opt.Ignore())
                .ForMember(dest => dest.CanDispose, opt => opt.Ignore());

            // Audit log mapping
            CreateMap<WasteAuditLog, WasteAuditLogDto>();
        }

        private static WasteClassification MapCategoryToClassification(WasteCategory category)
        {
            return category switch
            {
                WasteCategory.Hazardous => WasteClassification.HazardousChemical,
                WasteCategory.NonHazardous => WasteClassification.NonHazardous,
                WasteCategory.Recyclable => WasteClassification.Recyclable,
                _ => WasteClassification.NonHazardous
            };
        }

        private static string MapCategoryToString(WasteCategory category)
        {
            return category switch
            {
                WasteCategory.Hazardous => "Hazardous",
                WasteCategory.NonHazardous => "NonHazardous",
                WasteCategory.Recyclable => "Recyclable",
                _ => "NonHazardous"
            };
        }

        private static string MapDisposalStatusToString(WasteDisposalStatus disposalStatus)
        {
            return disposalStatus switch
            {
                WasteDisposalStatus.Pending => "Pending",
                WasteDisposalStatus.InProgress => "InProgress",
                WasteDisposalStatus.Disposed => "Disposed",
                _ => "Pending"
            };
        }

        private static WasteReportStatus MapDisposalStatusToReportStatus(WasteDisposalStatus disposalStatus)
        {
            return disposalStatus switch
            {
                WasteDisposalStatus.Pending => WasteReportStatus.Submitted,
                WasteDisposalStatus.InProgress => WasteReportStatus.InTransit,
                WasteDisposalStatus.Disposed => WasteReportStatus.Disposed,
                _ => WasteReportStatus.Draft
            };
        }
    }
}