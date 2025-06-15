# Waste Management System Implementation Plan

## Executive Summary

This document provides a comprehensive implementation plan for the Waste Management System for the Harmoni360 application. The Waste Management module will enable comprehensive tracking, management, and compliance monitoring of waste generation, classification, handling, and disposal processes, expanding the current HSE scope to include the full HSSE (Health, Safety, Security, Environment) solution.

## System Overview

### Core Objectives
- **Waste Report Submission**: Enable staff to report waste generation incidents and quantities
- **Waste Classification**: Categorize waste types (hazardous, non-hazardous, recyclable, etc.)
- **Disposal Tracking**: Track waste from generation through final disposal
- **Document Management**: Attach and manage disposal certificates, manifests, and photos
- **Compliance Monitoring**: Ensure regulatory compliance and reporting
- **Integration with HSSE**: Connect with existing incident management and safety protocols
- **Analytics & Reporting**: Provide insights into waste generation patterns and costs

### Key Features
1. **Waste Report Creation & Management**
2. **Waste Classification System**
3. **Disposal Provider Management**
4. **Document Attachment System**
5. **Compliance Dashboard**
6. **Waste Tracking Workflow**
7. **Integration with Incident Management**
8. **Mobile Support for Field Reporting**

## Technical Architecture

### Domain Entities

```csharp
// Core Entities
public class WasteReport : BaseEntity, IAuditableEntity
{
    public string ReportNumber { get; private set; }
    public int ReporterId { get; private set; }
    public User Reporter { get; private set; }
    public DateTime ReportDate { get; private set; }
    public int LocationId { get; private set; }
    public Location Location { get; private set; }
    public int DepartmentId { get; private set; }
    public Department Department { get; private set; }
    public int WasteCategoryId { get; private set; }
    public WasteCategory WasteCategory { get; private set; }
    public int WasteTypeId { get; private set; }
    public WasteType WasteType { get; private set; }
    public decimal Quantity { get; private set; }
    public UnitOfMeasure Unit { get; private set; }
    public string Description { get; private set; }
    public WasteSource Source { get; private set; }
    public string? SourceDetails { get; private set; }
    public bool IsHazardous { get; private set; }
    public string? HazardClassification { get; private set; }
    public WasteReportStatus Status { get; private set; }
    public DateTime? DisposalDate { get; private set; }
    public int? DisposalProviderId { get; private set; }
    public DisposalProvider? DisposalProvider { get; private set; }
    public string? DisposalMethod { get; private set; }
    public string? DisposalReferenceNumber { get; private set; }
    public decimal? DisposalCost { get; private set; }
    public string? RegulatoryCompliance { get; private set; }
    public int? RelatedIncidentId { get; private set; }
    public Incident? RelatedIncident { get; private set; }
    
    // Navigation properties
    public virtual ICollection<WasteAttachment> Attachments { get; private set; }
    public virtual ICollection<WasteDisposalRecord> DisposalRecords { get; private set; }
    public virtual ICollection<WasteComment> Comments { get; private set; }
}

public class WasteCategory : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }
    public WasteClassification Classification { get; private set; }
    public bool RequiresSpecialHandling { get; private set; }
    public bool RequiresManifest { get; private set; }
    public string? HandlingInstructions { get; private set; }
    public string? RegulatoryRequirements { get; private set; }
    public string? DisposalGuidelines { get; private set; }
    public bool IsActive { get; private set; }
    
    // Navigation properties
    public virtual ICollection<WasteType> WasteTypes { get; private set; }
}

public class WasteType : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }
    public int CategoryId { get; private set; }
    public WasteCategory Category { get; private set; }
    public string? WasteCode { get; private set; } // Regulatory waste code
    public string? UNNumber { get; private set; } // For hazardous waste
    public string? PhysicalState { get; private set; }
    public bool IsRecyclable { get; private set; }
    public string? RecyclingInstructions { get; private set; }
    public string? StorageRequirements { get; private set; }
    public int? MaxStorageDays { get; private set; }
    public bool IsActive { get; private set; }
}

public class DisposalProvider : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string LicenseNumber { get; private set; }
    public DateTime LicenseExpiryDate { get; private set; }
    public string ContactPerson { get; private set; }
    public string ContactPhone { get; private set; }
    public string ContactEmail { get; private set; }
    public string Address { get; private set; }
    public List<WasteClassification> AcceptedWasteTypes { get; private set; }
    public List<string> Certifications { get; private set; }
    public ProviderStatus Status { get; private set; }
    public decimal? BaseRate { get; private set; }
    public string? RateUnit { get; private set; }
    public string? Notes { get; private set; }
    
    // Navigation properties
    public virtual ICollection<WasteDisposalRecord> DisposalRecords { get; private set; }
}

public class WasteDisposalRecord : BaseEntity, IAuditableEntity
{
    public int WasteReportId { get; private set; }
    public WasteReport WasteReport { get; private set; }
    public int DisposalProviderId { get; private set; }
    public DisposalProvider DisposalProvider { get; private set; }
    public DateTime PickupDate { get; private set; }
    public DateTime DisposalDate { get; private set; }
    public string ManifestNumber { get; private set; }
    public decimal ActualQuantity { get; private set; }
    public UnitOfMeasure Unit { get; private set; }
    public string DisposalMethod { get; private set; }
    public string FacilityName { get; private set; }
    public string? FacilityAddress { get; private set; }
    public decimal Cost { get; private set; }
    public string? InvoiceNumber { get; private set; }
    public DisposalStatus Status { get; private set; }
    public string? CertificateNumber { get; private set; }
    public DateTime? CertificateDate { get; private set; }
    public string? RegulatoryApproval { get; private set; }
    public string? Notes { get; private set; }
}

public class WasteAttachment : BaseEntity, IAuditableEntity
{
    public int WasteReportId { get; private set; }
    public WasteReport WasteReport { get; private set; }
    public string FileName { get; private set; }
    public string FilePath { get; private set; }
    public long FileSize { get; private set; }
    public string ContentType { get; private set; }
    public AttachmentType Type { get; private set; }
    public string? Description { get; private set; }
    public int UploadedById { get; private set; }
    public User UploadedBy { get; private set; }
    public DateTime UploadedDate { get; private set; }
}

public class WasteComment : BaseEntity, IAuditableEntity
{
    public int WasteReportId { get; private set; }
    public WasteReport WasteReport { get; private set; }
    public string Comment { get; private set; }
    public int CommentedById { get; private set; }
    public User CommentedBy { get; private set; }
    public DateTime CommentedDate { get; private set; }
    public CommentType Type { get; private set; }
}

public class WasteCompliance : BaseEntity, IAuditableEntity
{
    public string RegulatoryBody { get; private set; }
    public string RegulationCode { get; private set; }
    public string RegulationName { get; private set; }
    public string Description { get; private set; }
    public List<WasteClassification> ApplicableWasteTypes { get; private set; }
    public string ComplianceRequirements { get; private set; }
    public int ReportingFrequencyDays { get; private set; }
    public DateTime? LastReportDate { get; private set; }
    public DateTime NextReportDueDate { get; private set; }
    public ComplianceStatus Status { get; private set; }
    public bool IsActive { get; private set; }
}
```

### Value Objects

```csharp
public class WasteQuantity : ValueObject
{
    public decimal Amount { get; private set; }
    public UnitOfMeasure Unit { get; private set; }
    public decimal? ConvertedToKg { get; private set; }
    public decimal? ConvertedToLiters { get; private set; }
}

public class DisposalCertificate : ValueObject
{
    public string CertificateNumber { get; private set; }
    public DateTime IssueDate { get; private set; }
    public string IssuingAuthority { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public string VerificationUrl { get; private set; }
}
```

### Enumerations

```csharp
public enum WasteClassification
{
    NonHazardous = 1,
    HazardousChemical = 2,
    HazardousBiological = 3,
    HazardousRadioactive = 4,
    Recyclable = 5,
    Organic = 6,
    Electronic = 7,
    Construction = 8,
    Medical = 9,
    Universal = 10
}

public enum WasteSource
{
    Laboratory = 1,
    Cafeteria = 2,
    Office = 3,
    Maintenance = 4,
    Construction = 5,
    Medical = 6,
    Classroom = 7,
    Event = 8,
    Other = 9
}

public enum UnitOfMeasure
{
    Kilogram = 1,
    Liter = 2,
    CubicMeter = 3,
    Ton = 4,
    Gallon = 5,
    Pound = 6,
    Unit = 7,
    Container = 8
}

public enum WasteReportStatus
{
    Draft = 1,
    Submitted = 2,
    UnderReview = 3,
    Approved = 4,
    InStorage = 5,
    AwaitingPickup = 6,
    InTransit = 7,
    Disposed = 8,
    Rejected = 9,
    Cancelled = 10
}

public enum DisposalStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    CertificatePending = 4,
    Certified = 5,
    Failed = 6
}

public enum AttachmentType
{
    Photo = 1,
    Manifest = 2,
    Certificate = 3,
    Invoice = 4,
    Permit = 5,
    Report = 6,
    Other = 7
}

public enum CommentType
{
    General = 1,
    StatusUpdate = 2,
    ComplianceNote = 3,
    DisposalUpdate = 4,
    Correction = 5
}

public enum ProviderStatus
{
    Active = 1,
    Suspended = 2,
    Expired = 3,
    UnderReview = 4,
    Terminated = 5
}

public enum ComplianceStatus
{
    Compliant = 1,
    NonCompliant = 2,
    PendingReview = 3,
    Overdue = 4
}
```

## API Endpoints

### Waste Report Management
```
GET    /api/waste-reports                    - Get waste reports with filtering/pagination
POST   /api/waste-reports                    - Create new waste report
GET    /api/waste-reports/{id}               - Get specific waste report
PUT    /api/waste-reports/{id}               - Update waste report
DELETE /api/waste-reports/{id}               - Delete waste report (soft delete)
GET    /api/waste-reports/my                 - Get user's waste reports
```

### Waste Report Operations
```
POST   /api/waste-reports/{id}/submit        - Submit draft report for review
POST   /api/waste-reports/{id}/approve       - Approve waste report
POST   /api/waste-reports/{id}/reject        - Reject waste report
POST   /api/waste-reports/{id}/dispose       - Record disposal
POST   /api/waste-reports/{id}/cancel        - Cancel waste report
```

### Document Management
```
GET    /api/waste-reports/{id}/attachments   - Get report attachments
POST   /api/waste-reports/{id}/attachments   - Upload attachment
GET    /api/waste-reports/attachments/{id}   - Download attachment
DELETE /api/waste-reports/attachments/{id}   - Delete attachment
```

### Comments
```
GET    /api/waste-reports/{id}/comments      - Get report comments
POST   /api/waste-reports/{id}/comments      - Add comment
```

### Disposal Management
```
GET    /api/waste/disposal-providers         - Get disposal providers
POST   /api/waste/disposal-providers         - Create disposal provider
PUT    /api/waste/disposal-providers/{id}    - Update disposal provider
DELETE /api/waste/disposal-providers/{id}    - Delete disposal provider
GET    /api/waste/disposal-records           - Get disposal records
POST   /api/waste/disposal-records           - Create disposal record
```

### Analytics & Reporting
```
GET    /api/waste/dashboard                  - Get dashboard metrics
GET    /api/waste/analytics/trends           - Get waste generation trends
GET    /api/waste/analytics/by-category      - Get waste by category
GET    /api/waste/analytics/costs            - Get disposal cost analysis
GET    /api/waste/compliance/status          - Get compliance status
GET    /api/waste/reports/summary            - Get summary reports
```

### Configuration
```
GET    /api/waste/categories                 - Get waste categories
POST   /api/waste/categories                 - Create waste category
PUT    /api/waste/categories/{id}            - Update waste category
DELETE /api/waste/categories/{id}            - Delete waste category

GET    /api/waste/types                      - Get waste types
POST   /api/waste/types                      - Create waste type
PUT    /api/waste/types/{id}                 - Update waste type
DELETE /api/waste/types/{id}                 - Delete waste type
```

## Application Layer Commands & Queries

### Commands
```csharp
// Waste Report Management
public record CreateWasteReportCommand : IRequest<WasteReportDto>
{
    public int LocationId { get; init; }
    public int DepartmentId { get; init; }
    public int WasteCategoryId { get; init; }
    public int WasteTypeId { get; init; }
    public decimal Quantity { get; init; }
    public UnitOfMeasure Unit { get; init; }
    public string Description { get; init; }
    public WasteSource Source { get; init; }
    public string? SourceDetails { get; init; }
    public bool IsHazardous { get; init; }
    public string? HazardClassification { get; init; }
    public int? RelatedIncidentId { get; init; }
}

public record UpdateWasteReportCommand : IRequest<WasteReportDto>
public record DeleteWasteReportCommand : IRequest<Unit>
public record SubmitWasteReportCommand : IRequest<Unit>
public record ApproveWasteReportCommand : IRequest<Unit>
public record RejectWasteReportCommand : IRequest<Unit>

// Disposal Management
public record RecordDisposalCommand : IRequest<WasteDisposalRecordDto>
public record UpdateDisposalRecordCommand : IRequest<WasteDisposalRecordDto>

// Attachment Management
public record UploadWasteAttachmentCommand : IRequest<WasteAttachmentDto>
public record DeleteWasteAttachmentCommand : IRequest<Unit>

// Comment Management
public record AddWasteCommentCommand : IRequest<WasteCommentDto>
```

### Queries
```csharp
// Waste Reports
public record GetWasteReportsQuery : IRequest<PaginatedList<WasteReportDto>>
{
    public WasteReportStatus? Status { get; init; }
    public int? CategoryId { get; init; }
    public int? TypeId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public record GetWasteReportByIdQuery : IRequest<WasteReportDetailDto>
public record GetMyWasteReportsQuery : IRequest<List<WasteReportDto>>

// Analytics
public record GetWasteDashboardQuery : IRequest<WasteDashboardDto>
public record GetWasteTrendsQuery : IRequest<WasteTrendsDto>
public record GetWasteCostAnalysisQuery : IRequest<WasteCostAnalysisDto>
public record GetComplianceStatusQuery : IRequest<ComplianceStatusDto>

// Configuration
public record GetWasteCategoriesQuery : IRequest<List<WasteCategoryDto>>
public record GetWasteTypesQuery : IRequest<List<WasteTypeDto>>
public record GetDisposalProvidersQuery : IRequest<List<DisposalProviderDto>>
```

## Frontend Components

### Page Structure
```
src/Harmoni360.Web/ClientApp/src/pages/waste-management/
├── WasteDashboard.tsx
├── WasteReportList.tsx
├── CreateWasteReport.tsx
├── EditWasteReport.tsx
├── WasteReportDetail.tsx
├── MyWasteReports.tsx
├── DisposalProviders.tsx
├── ComplianceDashboard.tsx
└── index.ts
```

### Core Views

1. **Waste Dashboard** (/waste-management/dashboard)
   - Overview metrics (Total reports, Pending disposal, This month's volume)
   - Waste classification breakdown chart
   - Recent waste reports table
   - Compliance status indicators
   - Quick actions (New report, View pending)
   - Cost analysis widget

2. **Waste Report List** (/waste-management)
   - Advanced filtering (status, category, date range, location)
   - Search functionality
   - Pagination with configurable page size
   - Quick actions (View, Edit, Download)
   - Export to Excel/PDF
   - Bulk operations support

3. **Create Waste Report** (/waste-management/create)
   - Multi-step form wizard
   - Basic Information (location, department, date)
   - Waste Details (category, type, quantity, source)
   - Hazard Classification (if applicable)
   - Related Incident linkage
   - File upload for photos/documents
   - Save as draft functionality

4. **Waste Report Detail** (/waste-management/:id)
   - Complete report information
   - Status workflow actions
   - Disposal records
   - Attached documents gallery
   - Comments/activity timeline
   - Print-friendly view
   - QR code for mobile access

5. **My Waste Reports** (/waste-management/my-reports)
   - User's submitted reports
   - Draft reports
   - Status filtering
   - Quick edit access
   - Performance metrics

6. **Disposal Providers** (/waste-management/providers)
   - Provider listing with search
   - License status indicators
   - Accepted waste types
   - Contact information
   - Performance ratings
   - Add/Edit provider forms

7. **Compliance Dashboard** (/waste-management/compliance)
   - Regulatory compliance status
   - Upcoming reporting deadlines
   - Non-compliance alerts
   - Audit trail
   - Report generation
   - Compliance trends

### Component Library

```typescript
// Common components
src/Harmoni360.Web/ClientApp/src/components/waste-management/
├── WasteReportForm.tsx
├── WasteAttachmentManager.tsx
├── DisposalRecordCard.tsx
├── ComplianceIndicator.tsx
├── WasteClassificationBadge.tsx
├── DisposalProviderSelector.tsx
├── WasteQuantityInput.tsx
├── WasteTrendChart.tsx
└── index.ts
```

## Database Schema

### Tables

1. **WasteReports**
   - Primary tracking table for all waste reports
   - Foreign keys to Users, Locations, Departments
   - Audit fields for tracking changes

2. **WasteCategories**
   - Master data for waste classifications
   - Regulatory requirements per category
   - Handling instructions

3. **WasteTypes**
   - Specific waste types under categories
   - Regulatory codes and UN numbers
   - Storage and recycling information

4. **DisposalProviders**
   - Licensed waste disposal companies
   - Certification tracking
   - Accepted waste types

5. **WasteDisposalRecords**
   - Disposal transaction records
   - Manifest and certificate tracking
   - Cost information

6. **WasteAttachments**
   - Document storage for reports
   - Support for multiple file types
   - Secure file access

7. **WasteComments**
   - Activity tracking
   - Status change history
   - User communications

8. **WasteCompliance**
   - Regulatory requirements
   - Reporting schedules
   - Compliance tracking

### Indexes
```sql
-- Performance indexes
CREATE INDEX IX_WasteReports_Status ON WasteReports(Status);
CREATE INDEX IX_WasteReports_ReportDate ON WasteReports(ReportDate);
CREATE INDEX IX_WasteReports_CategoryId ON WasteReports(WasteCategoryId);
CREATE INDEX IX_WasteDisposalRecords_DisposalDate ON WasteDisposalRecords(DisposalDate);
CREATE INDEX IX_WasteAttachments_WasteReportId ON WasteAttachments(WasteReportId);
```

## Integration Points

### HSSE Module Integration

1. **Incident Management**
   - Link waste reports to incidents
   - Automatic waste report creation from spill incidents
   - Cross-reference in reporting

2. **Safety Protocols**
   - PPE requirements for waste handling
   - Safety procedure references
   - Training requirements

3. **Risk Assessment**
   - Waste-related risk identification
   - Mitigation measures tracking
   - Environmental impact assessment

4. **Audit Management**
   - Waste management audit items
   - Compliance verification
   - Corrective action tracking

### System Integration

1. **Authentication & Authorization**
   - Role-based access control
   - Department-based visibility
   - Approval workflow permissions

2. **Notification System**
   - Email alerts for approvals
   - SMS for urgent disposals
   - Dashboard notifications

3. **Reporting Engine**
   - Monthly waste reports
   - Regulatory compliance reports
   - Cost analysis reports

## Implementation Phases

### Phase 1: Core Foundation (Week 1-2)
- [ ] Domain entities and value objects
- [ ] Database schema and migrations
- [ ] Basic CRUD operations
- [ ] File upload infrastructure

### Phase 2: Basic UI (Week 3-4)
- [ ] Waste report listing page
- [ ] Create/Edit waste report forms
- [ ] Basic dashboard
- [ ] File attachment UI

### Phase 3: Workflow Implementation (Week 5-6)
- [ ] Status workflow engine
- [ ] Approval process
- [ ] Disposal recording
- [ ] Email notifications

### Phase 4: Configuration Management (Week 7)
- [ ] Waste categories and types management
- [ ] Disposal provider management
- [ ] System settings

### Phase 5: Analytics & Reporting (Week 8-9)
- [ ] Dashboard analytics
- [ ] Trend analysis
- [ ] Cost tracking
- [ ] Report generation

### Phase 6: Compliance & Integration (Week 10-11)
- [ ] Compliance monitoring
- [ ] HSSE integration
- [ ] Mobile optimization
- [ ] Performance tuning

### Phase 7: Testing & Documentation (Week 12)
- [ ] Unit tests
- [ ] Integration tests
- [ ] User documentation
- [ ] Deployment preparation

## Security Considerations

1. **Data Access**
   - Department-based data isolation
   - Role-based feature access
   - Audit trail for all changes

2. **File Security**
   - Virus scanning for uploads
   - Secure file storage
   - Access control for downloads

3. **Compliance**
   - Data retention policies
   - Privacy protection
   - Regulatory requirements

## Performance Requirements

1. **Response Times**
   - Page load < 2 seconds
   - API response < 500ms
   - File upload < 30 seconds

2. **Scalability**
   - Support 1000+ reports/month
   - Handle 50+ concurrent users
   - Store 10GB+ attachments

3. **Availability**
   - 99.9% uptime target
   - Disaster recovery plan
   - Regular backups

## Success Metrics

1. **Adoption Metrics**
   - 90% waste report submission rate
   - 80% user adoption in 3 months
   - 95% disposal tracking accuracy

2. **Compliance Metrics**
   - 100% regulatory report submission
   - 0 compliance violations
   - 90% on-time disposal rate

3. **Efficiency Metrics**
   - 50% reduction in paper forms
   - 30% faster report processing
   - 40% cost reduction through analytics

## Conclusion

The Waste Management System will provide Harmoni360 with a comprehensive solution for tracking and managing waste from generation through disposal. By following the established architectural patterns and integrating seamlessly with existing HSSE modules, the system will enhance environmental compliance, reduce costs, and improve operational efficiency.

The modular design ensures future extensibility while maintaining consistency with the Harmoni360 platform standards. With proper implementation of the outlined features, the system will meet all regulatory requirements while providing an intuitive user experience for staff at all levels.
