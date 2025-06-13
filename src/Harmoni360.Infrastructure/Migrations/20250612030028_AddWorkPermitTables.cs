using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkPermitTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkPermits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PermitNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WorkLocation = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", precision: 10, scale: 8, nullable: true),
                    Longitude = table.Column<double>(type: "double precision", precision: 11, scale: 8, nullable: true),
                    PlannedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstimatedDuration = table.Column<int>(type: "integer", nullable: false),
                    RequestedById = table.Column<int>(type: "integer", nullable: false),
                    RequestedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequestedByDepartment = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequestedByPosition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WorkSupervisor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SafetyOfficer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WorkScope = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EquipmentToBeUsed = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MaterialsInvolved = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    NumberOfWorkers = table.Column<int>(type: "integer", nullable: false),
                    ContractorCompany = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RequiresHotWorkPermit = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresConfinedSpaceEntry = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresElectricalIsolation = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresHeightWork = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresRadiationWork = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresExcavation = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresFireWatch = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresGasMonitoring = table.Column<bool>(type: "boolean", nullable: false),
                    K3LicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CompanyWorkPermitNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsJamsostekCompliant = table.Column<bool>(type: "boolean", nullable: false),
                    HasSMK3Compliance = table.Column<bool>(type: "boolean", nullable: false),
                    EnvironmentalPermitNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RiskAssessmentSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EmergencyProcedures = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CompletionNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsCompletedSafely = table.Column<bool>(type: "boolean", nullable: false),
                    LessonsLearned = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkPermits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkPermitApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkPermitId = table.Column<int>(type: "integer", nullable: false),
                    ApprovedById = table.Column<int>(type: "integer", nullable: false),
                    ApprovedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ApprovalLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Signature = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApprovalOrder = table.Column<int>(type: "integer", nullable: false),
                    K3CertificateNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HasAuthorityToApprove = table.Column<bool>(type: "boolean", nullable: false),
                    AuthorityLevel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkPermitApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkPermitApprovals_WorkPermits_WorkPermitId",
                        column: x => x.WorkPermitId,
                        principalTable: "WorkPermits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkPermitAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkPermitId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UploadedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AttachmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkPermitAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkPermitAttachments_WorkPermits_WorkPermitId",
                        column: x => x.WorkPermitId,
                        principalTable: "WorkPermits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkPermitHazards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkPermitId = table.Column<int>(type: "integer", nullable: false),
                    HazardDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    RiskLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Likelihood = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Severity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ControlMeasures = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ResidualRiskLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResponsiblePerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsControlImplemented = table.Column<bool>(type: "boolean", nullable: false),
                    ControlImplementedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ImplementationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkPermitHazards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkPermitHazards_HazardCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "HazardCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkPermitHazards_WorkPermits_WorkPermitId",
                        column: x => x.WorkPermitId,
                        principalTable: "WorkPermits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkPermitPrecautions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkPermitId = table.Column<int>(type: "integer", nullable: false),
                    PrecautionDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CompletionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ResponsiblePerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VerificationMethod = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RequiresVerification = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsK3Requirement = table.Column<bool>(type: "boolean", nullable: false),
                    K3StandardReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsMandatoryByLaw = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkPermitPrecautions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkPermitPrecautions_WorkPermits_WorkPermitId",
                        column: x => x.WorkPermitId,
                        principalTable: "WorkPermits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitApprovals_ApprovalLevel",
                table: "WorkPermitApprovals",
                column: "ApprovalLevel");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitApprovals_ApprovedAt",
                table: "WorkPermitApprovals",
                column: "ApprovedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitApprovals_ApprovedById",
                table: "WorkPermitApprovals",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitApprovals_IsApproved",
                table: "WorkPermitApprovals",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitApprovals_WorkPermitId",
                table: "WorkPermitApprovals",
                column: "WorkPermitId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitApprovals_WorkPermitId_ApprovalLevel",
                table: "WorkPermitApprovals",
                columns: new[] { "WorkPermitId", "ApprovalLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitApprovals_WorkPermitId_ApprovalOrder",
                table: "WorkPermitApprovals",
                columns: new[] { "WorkPermitId", "ApprovalOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitAttachments_AttachmentType",
                table: "WorkPermitAttachments",
                column: "AttachmentType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitAttachments_UploadedAt",
                table: "WorkPermitAttachments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitAttachments_WorkPermitId",
                table: "WorkPermitAttachments",
                column: "WorkPermitId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitHazards_CategoryId",
                table: "WorkPermitHazards",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitHazards_IsControlImplemented",
                table: "WorkPermitHazards",
                column: "IsControlImplemented");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitHazards_RiskLevel",
                table: "WorkPermitHazards",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitHazards_WorkPermitId",
                table: "WorkPermitHazards",
                column: "WorkPermitId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitHazards_WorkPermitId_IsControlImplemented",
                table: "WorkPermitHazards",
                columns: new[] { "WorkPermitId", "IsControlImplemented" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitHazards_WorkPermitId_RiskLevel",
                table: "WorkPermitHazards",
                columns: new[] { "WorkPermitId", "RiskLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitPrecautions_Category",
                table: "WorkPermitPrecautions",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitPrecautions_IsCompleted",
                table: "WorkPermitPrecautions",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitPrecautions_IsK3Requirement",
                table: "WorkPermitPrecautions",
                column: "IsK3Requirement");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitPrecautions_IsK3Requirement_IsCompleted",
                table: "WorkPermitPrecautions",
                columns: new[] { "IsK3Requirement", "IsCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitPrecautions_IsMandatoryByLaw",
                table: "WorkPermitPrecautions",
                column: "IsMandatoryByLaw");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitPrecautions_IsRequired",
                table: "WorkPermitPrecautions",
                column: "IsRequired");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitPrecautions_IsVerified",
                table: "WorkPermitPrecautions",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitPrecautions_Priority",
                table: "WorkPermitPrecautions",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitPrecautions_WorkPermitId",
                table: "WorkPermitPrecautions",
                column: "WorkPermitId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitPrecautions_WorkPermitId_IsCompleted",
                table: "WorkPermitPrecautions",
                columns: new[] { "WorkPermitId", "IsCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitPrecautions_WorkPermitId_IsRequired",
                table: "WorkPermitPrecautions",
                columns: new[] { "WorkPermitId", "IsRequired" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitPrecautions_WorkPermitId_Priority",
                table: "WorkPermitPrecautions",
                columns: new[] { "WorkPermitId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermits_CreatedAt",
                table: "WorkPermits",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermits_PermitNumber",
                table: "WorkPermits",
                column: "PermitNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermits_PlannedEndDate",
                table: "WorkPermits",
                column: "PlannedEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermits_PlannedStartDate",
                table: "WorkPermits",
                column: "PlannedStartDate");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermits_PlannedStartDate_Status",
                table: "WorkPermits",
                columns: new[] { "PlannedStartDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermits_Priority",
                table: "WorkPermits",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermits_RequestedById",
                table: "WorkPermits",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermits_RequestedById_Status",
                table: "WorkPermits",
                columns: new[] { "RequestedById", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermits_RiskLevel",
                table: "WorkPermits",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermits_Status",
                table: "WorkPermits",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermits_Status_Type",
                table: "WorkPermits",
                columns: new[] { "Status", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermits_Type",
                table: "WorkPermits",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkPermitApprovals");

            migrationBuilder.DropTable(
                name: "WorkPermitAttachments");

            migrationBuilder.DropTable(
                name: "WorkPermitHazards");

            migrationBuilder.DropTable(
                name: "WorkPermitPrecautions");

            migrationBuilder.DropTable(
                name: "WorkPermits");
        }
    }
}
