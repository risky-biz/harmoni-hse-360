using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HarmoniHSE360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHazardManagementSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HazardAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HazardId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false, comment: "File size in bytes"),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HazardAttachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HazardMitigationActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HazardId = table.Column<int>(type: "integer", nullable: false),
                    ActionDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedToId = table.Column<int>(type: "integer", nullable: false),
                    CompletionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    EffectivenessRating = table.Column<int>(type: "integer", nullable: true, comment: "Effectiveness rating (1-5)"),
                    EffectivenessNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ActualCost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    RequiresVerification = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedById = table.Column<int>(type: "integer", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HazardMitigationActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HazardMitigationActions_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HazardMitigationActions_Users_VerifiedById",
                        column: x => x.VerifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HazardReassessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HazardId = table.Column<int>(type: "integer", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedById = table.Column<int>(type: "integer", nullable: true),
                    CompletionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HazardReassessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HazardReassessments_Users_CompletedById",
                        column: x => x.CompletedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Hazards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: true),
                    Longitude = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    IdentifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpectedResolutionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReporterId = table.Column<int>(type: "integer", nullable: false),
                    ReporterDepartment = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CurrentRiskAssessmentId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hazards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hazards_Users_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RiskAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HazardId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    AssessorId = table.Column<int>(type: "integer", nullable: false),
                    AssessmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProbabilityScore = table.Column<int>(type: "integer", nullable: false, comment: "Risk probability score (1-5)"),
                    SeverityScore = table.Column<int>(type: "integer", nullable: false, comment: "Risk severity score (1-5)"),
                    RiskScore = table.Column<int>(type: "integer", nullable: false, comment: "Calculated risk score (Probability × Severity)"),
                    RiskLevel = table.Column<string>(type: "text", nullable: false),
                    PotentialConsequences = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ExistingControls = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RecommendedActions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AdditionalNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    NextReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedById = table.Column<int>(type: "integer", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskAssessments_Hazards_HazardId",
                        column: x => x.HazardId,
                        principalTable: "Hazards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiskAssessments_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RiskAssessments_Users_AssessorId",
                        column: x => x.AssessorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HazardAttachments_ContentType",
                table: "HazardAttachments",
                column: "ContentType");

            migrationBuilder.CreateIndex(
                name: "IX_HazardAttachments_FileName",
                table: "HazardAttachments",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_HazardAttachments_HazardId",
                table: "HazardAttachments",
                column: "HazardId");

            migrationBuilder.CreateIndex(
                name: "IX_HazardAttachments_UploadedAt",
                table: "HazardAttachments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HazardMitigationActions_AssignedToId",
                table: "HazardMitigationActions",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_HazardMitigationActions_AssignedToId_Status",
                table: "HazardMitigationActions",
                columns: new[] { "AssignedToId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_HazardMitigationActions_HazardId",
                table: "HazardMitigationActions",
                column: "HazardId");

            migrationBuilder.CreateIndex(
                name: "IX_HazardMitigationActions_HazardId_Status",
                table: "HazardMitigationActions",
                columns: new[] { "HazardId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_HazardMitigationActions_Priority",
                table: "HazardMitigationActions",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_HazardMitigationActions_RequiresVerification",
                table: "HazardMitigationActions",
                column: "RequiresVerification");

            migrationBuilder.CreateIndex(
                name: "IX_HazardMitigationActions_Status",
                table: "HazardMitigationActions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_HazardMitigationActions_Status_TargetDate",
                table: "HazardMitigationActions",
                columns: new[] { "Status", "TargetDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HazardMitigationActions_TargetDate",
                table: "HazardMitigationActions",
                column: "TargetDate");

            migrationBuilder.CreateIndex(
                name: "IX_HazardMitigationActions_Type",
                table: "HazardMitigationActions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_HazardMitigationActions_VerifiedById",
                table: "HazardMitigationActions",
                column: "VerifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_HazardReassessments_CompletedAt",
                table: "HazardReassessments",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HazardReassessments_CompletedById",
                table: "HazardReassessments",
                column: "CompletedById");

            migrationBuilder.CreateIndex(
                name: "IX_HazardReassessments_HazardId",
                table: "HazardReassessments",
                column: "HazardId");

            migrationBuilder.CreateIndex(
                name: "IX_HazardReassessments_IsCompleted",
                table: "HazardReassessments",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_HazardReassessments_IsCompleted_ScheduledDate",
                table: "HazardReassessments",
                columns: new[] { "IsCompleted", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HazardReassessments_ScheduledDate",
                table: "HazardReassessments",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_Category",
                table: "Hazards",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_Category_Status",
                table: "Hazards",
                columns: new[] { "Category", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_CreatedAt",
                table: "Hazards",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_CurrentRiskAssessmentId",
                table: "Hazards",
                column: "CurrentRiskAssessmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_IdentifiedDate",
                table: "Hazards",
                column: "IdentifiedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_ReporterDepartment",
                table: "Hazards",
                column: "ReporterDepartment");

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_ReporterId",
                table: "Hazards",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_Severity",
                table: "Hazards",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_Status",
                table: "Hazards",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_Status_Severity",
                table: "Hazards",
                columns: new[] { "Status", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_Type",
                table: "Hazards",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_ApprovedById",
                table: "RiskAssessments",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_AssessmentDate",
                table: "RiskAssessments",
                column: "AssessmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_AssessorId",
                table: "RiskAssessments",
                column: "AssessorId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_HazardId",
                table: "RiskAssessments",
                column: "HazardId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_HazardId_IsActive",
                table: "RiskAssessments",
                columns: new[] { "HazardId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_IsActive",
                table: "RiskAssessments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_IsApproved",
                table: "RiskAssessments",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_NextReviewDate",
                table: "RiskAssessments",
                column: "NextReviewDate");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_RiskLevel",
                table: "RiskAssessments",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_RiskLevel_IsActive",
                table: "RiskAssessments",
                columns: new[] { "RiskLevel", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_RiskScore",
                table: "RiskAssessments",
                column: "RiskScore");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessments_Type",
                table: "RiskAssessments",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_HazardAttachments_Hazards_HazardId",
                table: "HazardAttachments",
                column: "HazardId",
                principalTable: "Hazards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HazardMitigationActions_Hazards_HazardId",
                table: "HazardMitigationActions",
                column: "HazardId",
                principalTable: "Hazards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HazardReassessments_Hazards_HazardId",
                table: "HazardReassessments",
                column: "HazardId",
                principalTable: "Hazards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Hazards_RiskAssessments_CurrentRiskAssessmentId",
                table: "Hazards",
                column: "CurrentRiskAssessmentId",
                principalTable: "RiskAssessments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RiskAssessments_Hazards_HazardId",
                table: "RiskAssessments");

            migrationBuilder.DropTable(
                name: "HazardAttachments");

            migrationBuilder.DropTable(
                name: "HazardMitigationActions");

            migrationBuilder.DropTable(
                name: "HazardReassessments");

            migrationBuilder.DropTable(
                name: "Hazards");

            migrationBuilder.DropTable(
                name: "RiskAssessments");
        }
    }
}
