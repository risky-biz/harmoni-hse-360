using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingEntitiesClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Trainings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainingCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeliveryMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScheduledStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScheduledEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationHours = table.Column<int>(type: "integer", nullable: false),
                    MaxParticipants = table.Column<int>(type: "integer", nullable: false),
                    MinParticipants = table.Column<int>(type: "integer", nullable: false),
                    Venue = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    VenueAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", precision: 10, scale: 8, nullable: true),
                    Longitude = table.Column<double>(type: "double precision", precision: 11, scale: 8, nullable: true),
                    OnlineLink = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OnlinePlatform = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LearningObjectives = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CourseOutline = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Prerequisites = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Materials = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AssessmentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PassingScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    InstructorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InstructorQualifications = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    InstructorContact = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsExternalInstructor = table.Column<bool>(type: "boolean", nullable: false),
                    IssuesCertificate = table.Column<bool>(type: "boolean", nullable: false),
                    CertificationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CertificateValidityPeriod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CertifyingBody = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CostPerParticipant = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalBudget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "IDR"),
                    IsK3MandatoryTraining = table.Column<bool>(type: "boolean", nullable: false),
                    K3RegulationReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsBPJSCompliant = table.Column<bool>(type: "boolean", nullable: false),
                    MinistryApprovalNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequiresGovernmentCertification = table.Column<bool>(type: "boolean", nullable: false),
                    IndonesianTrainingStandard = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    TotalRatings = table.Column<int>(type: "integer", nullable: false),
                    EvaluationSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ImprovementActions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainingCertifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainingId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CertificateNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CertificationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CertifyingBody = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CertificateTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsValid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RevokedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RevocationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FinalScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    PassingScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Grade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PerformanceNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IssuedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IssuedByTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IssuedByOrganization = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IssuerLicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DigitalSignature = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsK3Certificate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    K3CertificateType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    K3LicenseClass = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsGovernmentRecognized = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MinistryApprovalNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IndonesianStandardReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsBPJSCompliant = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    BPJSReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CertificateFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CertificateFileHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    QRCodeData = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    VerificationUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HasWatermark = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RequiresRenewal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RenewalDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RenewalReminderSent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RenewalRequirements = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RenewedFromCertificateId = table.Column<int>(type: "integer", nullable: true),
                    IsRenewal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    VerificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VerificationMethod = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VerificationAttempts = table.Column<int>(type: "integer", nullable: false),
                    LastVerificationAttempt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CPDCreditsEarned = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    CPDCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CountsTowardsCPD = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ProfessionalBodyReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActiveCredential = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    UsageRestrictions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    GeographicScope = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Indonesia"),
                    IndustryScope = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LastAuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAuditResult = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RequiresPeriodicAudit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ComplianceStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Compliant"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingCertifications_TrainingCertifications_RenewedFromCe~",
                        column: x => x.RenewedFromCertificateId,
                        principalTable: "TrainingCertifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TrainingCertifications_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainingId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CommentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AuthorId = table.Column<int>(type: "integer", nullable: true),
                    AuthorRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CommentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ParentCommentId = table.Column<int>(type: "integer", nullable: true),
                    IsReply = table.Column<bool>(type: "boolean", nullable: false),
                    ReplyCount = table.Column<int>(type: "integer", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsInstructorOnly = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPrivateNote = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSystemGenerated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsImportant = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RequiresResponse = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ResolvedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsComplianceNote = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RegulatoryContext = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsK3Related = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    K3IssueType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RelatedRating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: true),
                    FeedbackCategory = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AttachmentPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReferencedDocuments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsModerated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ModeratedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModerationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModerationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsEdited = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastEditedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EditReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EditCount = table.Column<int>(type: "integer", nullable: false),
                    LikeCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsPinned = table.Column<bool>(type: "boolean", nullable: false),
                    PinnedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PinnedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingComments_TrainingComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "TrainingComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainingComments_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainingId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserDepartment = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserPosition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnrolledBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AttendanceStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttendanceEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttendancePercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AttendanceNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FinalScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    HasPassed = table.Column<bool>(type: "boolean", nullable: false),
                    AssessmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssessmentNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AssessmentMethodUsed = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TrainingRating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: true),
                    TrainingFeedback = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    InstructorFeedback = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    HasMetPrerequisites = table.Column<bool>(type: "boolean", nullable: false),
                    PrerequisiteNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PrerequisiteVerificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmployeeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    K3LicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsBPJSRegistered = table.Column<bool>(type: "boolean", nullable: false),
                    BPJSNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsIndonesianCitizen = table.Column<bool>(type: "boolean", nullable: false),
                    WorkPermitNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsEligibleForCertificate = table.Column<bool>(type: "boolean", nullable: false),
                    CompletionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CertificateIssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CertificateNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingParticipants_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainingId = table.Column<int>(type: "integer", nullable: false),
                    RequirementDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsOverdue = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedTo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AssignedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CompletionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    VerificationMethod = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RequiresVerification = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VerificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsK3Requirement = table.Column<bool>(type: "boolean", nullable: false),
                    K3RegulationReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsGovernmentMandated = table.Column<bool>(type: "boolean", nullable: false),
                    RegulatoryReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsBPJSRelated = table.Column<bool>(type: "boolean", nullable: false),
                    DocumentationRequired = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    EvidenceProvided = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AttachmentPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RiskLevelIfNotCompleted = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ComplianceNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ComplianceCost = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingRequirements_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrainingId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UploadedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AttachmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsInstructorOnly = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsParticipantSubmission = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SubmittedByParticipantId = table.Column<int>(type: "integer", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    VersionNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PreviousVersionId = table.Column<int>(type: "integer", nullable: true),
                    IsCurrentVersion = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsComplianceDocument = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RegulatoryReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsK3Document = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    K3DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "id-ID"),
                    IsTranslationRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TranslatedFrom = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    HasDigitalSignature = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SignatureInfo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DownloadCount = table.Column<int>(type: "integer", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAccessedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ChecksumMD5 = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ChecksumSHA256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsVirusScanned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VirusScanDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsVirusClean = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingAttachments_TrainingParticipants_SubmittedByPartici~",
                        column: x => x.SubmittedByParticipantId,
                        principalTable: "TrainingParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TrainingAttachments_Trainings_TrainingId",
                        column: x => x.TrainingId,
                        principalTable: "Trainings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAttachments_AttachmentType",
                table: "TrainingAttachments",
                column: "AttachmentType");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAttachments_IsApproved",
                table: "TrainingAttachments",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAttachments_IsComplianceDocument",
                table: "TrainingAttachments",
                column: "IsComplianceDocument");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAttachments_IsCurrentVersion",
                table: "TrainingAttachments",
                column: "IsCurrentVersion");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAttachments_IsPublic",
                table: "TrainingAttachments",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAttachments_IsPublic_IsApproved",
                table: "TrainingAttachments",
                columns: new[] { "IsPublic", "IsApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAttachments_SubmittedByParticipantId",
                table: "TrainingAttachments",
                column: "SubmittedByParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAttachments_TrainingId",
                table: "TrainingAttachments",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAttachments_TrainingId_AttachmentType",
                table: "TrainingAttachments",
                columns: new[] { "TrainingId", "AttachmentType" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAttachments_TrainingId_IsCurrentVersion",
                table: "TrainingAttachments",
                columns: new[] { "TrainingId", "IsCurrentVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAttachments_TrainingId_IsK3Document",
                table: "TrainingAttachments",
                columns: new[] { "TrainingId", "IsK3Document" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingAttachments_UploadedAt",
                table: "TrainingAttachments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_CertificateNumber",
                table: "TrainingCertifications",
                column: "CertificateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_IsK3Certificate",
                table: "TrainingCertifications",
                column: "IsK3Certificate");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_IsK3Certificate_K3CertificateType",
                table: "TrainingCertifications",
                columns: new[] { "IsK3Certificate", "K3CertificateType" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_IsRevoked",
                table: "TrainingCertifications",
                column: "IsRevoked");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_IssuedDate",
                table: "TrainingCertifications",
                column: "IssuedDate");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_IsValid",
                table: "TrainingCertifications",
                column: "IsValid");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_IsValid_ValidUntil",
                table: "TrainingCertifications",
                columns: new[] { "IsValid", "ValidUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_RenewalDueDate",
                table: "TrainingCertifications",
                column: "RenewalDueDate");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_RenewedFromCertificateId",
                table: "TrainingCertifications",
                column: "RenewedFromCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_RequiresRenewal",
                table: "TrainingCertifications",
                column: "RequiresRenewal");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_RequiresRenewal_RenewalDueDate",
                table: "TrainingCertifications",
                columns: new[] { "RequiresRenewal", "RenewalDueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_TrainingId",
                table: "TrainingCertifications",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_TrainingId_UserId",
                table: "TrainingCertifications",
                columns: new[] { "TrainingId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_UserId",
                table: "TrainingCertifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_UserId_IsValid",
                table: "TrainingCertifications",
                columns: new[] { "UserId", "IsValid" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_ValidUntil",
                table: "TrainingCertifications",
                column: "ValidUntil");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingComments_AuthorId",
                table: "TrainingComments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingComments_AuthorId_CommentDate",
                table: "TrainingComments",
                columns: new[] { "AuthorId", "CommentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingComments_CommentDate",
                table: "TrainingComments",
                column: "CommentDate");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingComments_CommentType",
                table: "TrainingComments",
                column: "CommentType");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingComments_IsImportant",
                table: "TrainingComments",
                column: "IsImportant");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingComments_IsPinned",
                table: "TrainingComments",
                column: "IsPinned");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingComments_IsPublic",
                table: "TrainingComments",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingComments_ParentCommentId",
                table: "TrainingComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingComments_TrainingId",
                table: "TrainingComments",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingComments_TrainingId_CommentType",
                table: "TrainingComments",
                columns: new[] { "TrainingId", "CommentType" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingComments_TrainingId_IsPinned",
                table: "TrainingComments",
                columns: new[] { "TrainingId", "IsPinned" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingComments_TrainingId_IsPublic",
                table: "TrainingComments",
                columns: new[] { "TrainingId", "IsPublic" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingParticipants_EnrolledAt",
                table: "TrainingParticipants",
                column: "EnrolledAt");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingParticipants_IsEligibleForCertificate",
                table: "TrainingParticipants",
                column: "IsEligibleForCertificate");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingParticipants_Status",
                table: "TrainingParticipants",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingParticipants_Status_IsEligibleForCertificate",
                table: "TrainingParticipants",
                columns: new[] { "Status", "IsEligibleForCertificate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingParticipants_TrainingId",
                table: "TrainingParticipants",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingParticipants_TrainingId_Status",
                table: "TrainingParticipants",
                columns: new[] { "TrainingId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingParticipants_TrainingId_UserId",
                table: "TrainingParticipants",
                columns: new[] { "TrainingId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingParticipants_UserId",
                table: "TrainingParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingParticipants_UserId_Status",
                table: "TrainingParticipants",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRequirements_DueDate",
                table: "TrainingRequirements",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRequirements_IsMandatory",
                table: "TrainingRequirements",
                column: "IsMandatory");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRequirements_IsVerified",
                table: "TrainingRequirements",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRequirements_Status",
                table: "TrainingRequirements",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRequirements_Status_DueDate",
                table: "TrainingRequirements",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRequirements_TrainingId",
                table: "TrainingRequirements",
                column: "TrainingId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRequirements_TrainingId_IsMandatory",
                table: "TrainingRequirements",
                columns: new[] { "TrainingId", "IsMandatory" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRequirements_TrainingId_Status",
                table: "TrainingRequirements",
                columns: new[] { "TrainingId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_AvailableSpots",
                table: "Trainings",
                columns: new[] { "Status", "MaxParticipants" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_Category",
                table: "Trainings",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_Category_Priority",
                table: "Trainings",
                columns: new[] { "Category", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_CreatedAt",
                table: "Trainings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_DeliveryMethod",
                table: "Trainings",
                column: "DeliveryMethod");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_DeliveryMethod_Status",
                table: "Trainings",
                columns: new[] { "DeliveryMethod", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_InstructorName_Search",
                table: "Trainings",
                column: "InstructorName");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_IsK3MandatoryTraining_Status",
                table: "Trainings",
                columns: new[] { "IsK3MandatoryTraining", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_Overdue_Query",
                table: "Trainings",
                columns: new[] { "Status", "ScheduledStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_Priority",
                table: "Trainings",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_ScheduledEndDate",
                table: "Trainings",
                column: "ScheduledEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_ScheduledStartDate",
                table: "Trainings",
                column: "ScheduledStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_Status",
                table: "Trainings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_Status_Priority_ScheduledStartDate",
                table: "Trainings",
                columns: new[] { "Status", "Priority", "ScheduledStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_Status_Type",
                table: "Trainings",
                columns: new[] { "Status", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_Title_Search",
                table: "Trainings",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_TrainingCode",
                table: "Trainings",
                column: "TrainingCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_Type",
                table: "Trainings",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_Type_Category",
                table: "Trainings",
                columns: new[] { "Type", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_Type_Status_CreatedAt",
                table: "Trainings",
                columns: new[] { "Type", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_Venue_Search",
                table: "Trainings",
                column: "Venue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainingAttachments");

            migrationBuilder.DropTable(
                name: "TrainingCertifications");

            migrationBuilder.DropTable(
                name: "TrainingComments");

            migrationBuilder.DropTable(
                name: "TrainingRequirements");

            migrationBuilder.DropTable(
                name: "TrainingParticipants");

            migrationBuilder.DropTable(
                name: "Trainings");
        }
    }
}
