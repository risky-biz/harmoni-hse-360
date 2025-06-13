using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityIncidentManagementSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Roles_RoleId",
                table: "UserRole");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Users_UserId",
                table: "UserRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRole",
                table: "UserRole");

            migrationBuilder.RenameTable(
                name: "UserRole",
                newName: "UserRoles");

            migrationBuilder.RenameIndex(
                name: "IX_UserRole_UserId_RoleId",
                table: "UserRoles",
                newName: "IX_UserRoles_UserId_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRoles",
                newName: "IX_UserRoles_RoleId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserRoles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserRoles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "UserRoles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "UserRoles",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SecurityIncidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncidentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IncidentType = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ThreatLevel = table.Column<int>(type: "integer", nullable: false),
                    IncidentDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DetectionDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "numeric(10,8)", nullable: true),
                    Longitude = table.Column<double>(type: "numeric(11,8)", nullable: true),
                    ThreatActorType = table.Column<int>(type: "integer", nullable: true),
                    ThreatActorDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsInternalThreat = table.Column<bool>(type: "boolean", nullable: false),
                    Impact = table.Column<int>(type: "integer", nullable: false),
                    AffectedPersonsCount = table.Column<int>(type: "integer", nullable: true),
                    EstimatedLoss = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    DataBreachOccurred = table.Column<bool>(type: "boolean", nullable: false),
                    ContainmentDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolutionDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ContainmentActions = table.Column<string>(type: "text", nullable: true),
                    RootCause = table.Column<string>(type: "text", nullable: true),
                    ReporterId = table.Column<int>(type: "integer", nullable: false),
                    AssignedToId = table.Column<int>(type: "integer", nullable: true),
                    InvestigatorId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityIncidents_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SecurityIncidents_Users_InvestigatorId",
                        column: x => x.InvestigatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SecurityIncidents_Users_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThreatIndicators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IndicatorType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IndicatorValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThreatType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Confidence = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FirstSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Tags = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreatIndicators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityControls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ControlName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ControlDescription = table.Column<string>(type: "text", nullable: false),
                    ControlType = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ImplementationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectivenessScore = table.Column<int>(type: "integer", nullable: true),
                    EffectivenessNotes = table.Column<string>(type: "text", nullable: true),
                    ImplementationCost = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    AnnualMaintenanceCost = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    RelatedIncidentId = table.Column<int>(type: "integer", nullable: true),
                    ImplementedById = table.Column<int>(type: "integer", nullable: false),
                    ReviewedById = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityControls_SecurityIncidents_RelatedIncidentId",
                        column: x => x.RelatedIncidentId,
                        principalTable: "SecurityIncidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SecurityControls_Users_ImplementedById",
                        column: x => x.ImplementedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SecurityControls_Users_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SecurityIncidentAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SecurityIncidentId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AttachmentType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsConfidential = table.Column<bool>(type: "boolean", nullable: false),
                    Hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityIncidentAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityIncidentAttachments_SecurityIncidents_SecurityIncid~",
                        column: x => x.SecurityIncidentId,
                        principalTable: "SecurityIncidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityIncidentInvolvedPersons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SecurityIncidentId = table.Column<int>(type: "integer", nullable: false),
                    PersonId = table.Column<int>(type: "integer", nullable: false),
                    Involvement = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsWitness = table.Column<bool>(type: "boolean", nullable: false),
                    IsVictim = table.Column<bool>(type: "boolean", nullable: false),
                    IsSuspect = table.Column<bool>(type: "boolean", nullable: false),
                    IsReporter = table.Column<bool>(type: "boolean", nullable: false),
                    Statement = table.Column<string>(type: "text", nullable: true),
                    StatementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StatementTaken = table.Column<bool>(type: "boolean", nullable: false),
                    ContactMethod = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AdditionalNotes = table.Column<string>(type: "text", nullable: true),
                    InvolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AddedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityIncidentInvolvedPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityIncidentInvolvedPersons_SecurityIncidents_SecurityI~",
                        column: x => x.SecurityIncidentId,
                        principalTable: "SecurityIncidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SecurityIncidentInvolvedPersons_Users_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SecurityIncidentResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SecurityIncidentId = table.Column<int>(type: "integer", nullable: false),
                    ResponseType = table.Column<int>(type: "integer", nullable: false),
                    ActionTaken = table.Column<string>(type: "text", nullable: false),
                    ActionDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WasSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    FollowUpRequired = table.Column<bool>(type: "boolean", nullable: false),
                    FollowUpDetails = table.Column<string>(type: "text", nullable: true),
                    FollowUpDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffortHours = table.Column<int>(type: "integer", nullable: true),
                    Cost = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    ToolsUsed = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResourcesUsed = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResponderId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityIncidentResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityIncidentResponses_SecurityIncidents_SecurityInciden~",
                        column: x => x.SecurityIncidentId,
                        principalTable: "SecurityIncidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SecurityIncidentResponses_Users_ResponderId",
                        column: x => x.ResponderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThreatAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SecurityIncidentId = table.Column<int>(type: "integer", nullable: false),
                    CurrentThreatLevel = table.Column<int>(type: "integer", nullable: false),
                    PreviousThreatLevel = table.Column<int>(type: "integer", nullable: false),
                    AssessmentRationale = table.Column<string>(type: "text", nullable: false),
                    AssessmentDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExternalThreatIntelUsed = table.Column<bool>(type: "boolean", nullable: false),
                    ThreatIntelSource = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ThreatIntelDetails = table.Column<string>(type: "text", nullable: true),
                    ThreatCapability = table.Column<int>(type: "integer", nullable: false),
                    ThreatIntent = table.Column<int>(type: "integer", nullable: false),
                    TargetVulnerability = table.Column<int>(type: "integer", nullable: false),
                    ImpactPotential = table.Column<int>(type: "integer", nullable: false),
                    AssessedById = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreatAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThreatAssessments_SecurityIncidents_SecurityIncidentId",
                        column: x => x.SecurityIncidentId,
                        principalTable: "SecurityIncidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThreatAssessments_Users_AssessedById",
                        column: x => x.AssessedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SecurityIncidentIndicators",
                columns: table => new
                {
                    SecurityIncidentId = table.Column<int>(type: "integer", nullable: false),
                    ThreatIndicatorId = table.Column<int>(type: "integer", nullable: false),
                    Context = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityIncidentIndicators", x => new { x.SecurityIncidentId, x.ThreatIndicatorId });
                    table.ForeignKey(
                        name: "FK_SecurityIncidentIndicators_SecurityIncidents_SecurityIncide~",
                        column: x => x.SecurityIncidentId,
                        principalTable: "SecurityIncidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SecurityIncidentIndicators_ThreatIndicators_ThreatIndicator~",
                        column: x => x.ThreatIndicatorId,
                        principalTable: "ThreatIndicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityIncidentControls",
                columns: table => new
                {
                    SecurityControlId = table.Column<int>(type: "integer", nullable: false),
                    SecurityIncidentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityIncidentControls", x => new { x.SecurityControlId, x.SecurityIncidentId });
                    table.ForeignKey(
                        name: "FK_SecurityIncidentControls_SecurityControls_SecurityControlId",
                        column: x => x.SecurityControlId,
                        principalTable: "SecurityControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SecurityIncidentControls_SecurityIncidents_SecurityIncident~",
                        column: x => x.SecurityIncidentId,
                        principalTable: "SecurityIncidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityControls_Category",
                table: "SecurityControls",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityControls_ControlType",
                table: "SecurityControls",
                column: "ControlType");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityControls_ImplementedById",
                table: "SecurityControls",
                column: "ImplementedById");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityControls_NextReviewDate",
                table: "SecurityControls",
                column: "NextReviewDate");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityControls_RelatedIncidentId",
                table: "SecurityControls",
                column: "RelatedIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityControls_ReviewedById",
                table: "SecurityControls",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityControls_Status",
                table: "SecurityControls",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityControls_Status_NextReviewDate",
                table: "SecurityControls",
                columns: new[] { "Status", "NextReviewDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityControls_Type_Category",
                table: "SecurityControls",
                columns: new[] { "ControlType", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentAttachments_AttachmentType",
                table: "SecurityIncidentAttachments",
                column: "AttachmentType");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentAttachments_Incident_Type",
                table: "SecurityIncidentAttachments",
                columns: new[] { "SecurityIncidentId", "AttachmentType" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentAttachments_IsConfidential",
                table: "SecurityIncidentAttachments",
                column: "IsConfidential");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentAttachments_SecurityIncidentId",
                table: "SecurityIncidentAttachments",
                column: "SecurityIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentAttachments_UploadedAt",
                table: "SecurityIncidentAttachments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentControls_SecurityIncidentId",
                table: "SecurityIncidentControls",
                column: "SecurityIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentIndicators_ThreatIndicatorId",
                table: "SecurityIncidentIndicators",
                column: "ThreatIndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentInvolvedPersons_Incident_Person",
                table: "SecurityIncidentInvolvedPersons",
                columns: new[] { "SecurityIncidentId", "PersonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentInvolvedPersons_IsSuspect",
                table: "SecurityIncidentInvolvedPersons",
                column: "IsSuspect");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentInvolvedPersons_IsVictim",
                table: "SecurityIncidentInvolvedPersons",
                column: "IsVictim");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentInvolvedPersons_IsWitness",
                table: "SecurityIncidentInvolvedPersons",
                column: "IsWitness");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentInvolvedPersons_PersonId",
                table: "SecurityIncidentInvolvedPersons",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentInvolvedPersons_SecurityIncidentId",
                table: "SecurityIncidentInvolvedPersons",
                column: "SecurityIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentInvolvedPersons_StatementTaken",
                table: "SecurityIncidentInvolvedPersons",
                column: "StatementTaken");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentResponses_ActionDateTime",
                table: "SecurityIncidentResponses",
                column: "ActionDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentResponses_FollowUpDueDate",
                table: "SecurityIncidentResponses",
                column: "FollowUpDueDate");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentResponses_FollowUpRequired",
                table: "SecurityIncidentResponses",
                column: "FollowUpRequired");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentResponses_Incident_DateTime",
                table: "SecurityIncidentResponses",
                columns: new[] { "SecurityIncidentId", "ActionDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentResponses_Incident_Type",
                table: "SecurityIncidentResponses",
                columns: new[] { "SecurityIncidentId", "ResponseType" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentResponses_ResponderId",
                table: "SecurityIncidentResponses",
                column: "ResponderId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentResponses_ResponseType",
                table: "SecurityIncidentResponses",
                column: "ResponseType");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidentResponses_SecurityIncidentId",
                table: "SecurityIncidentResponses",
                column: "SecurityIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_AssignedToId",
                table: "SecurityIncidents",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_IncidentDateTime",
                table: "SecurityIncidents",
                column: "IncidentDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_IncidentNumber",
                table: "SecurityIncidents",
                column: "IncidentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_IncidentType",
                table: "SecurityIncidents",
                column: "IncidentType");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_InvestigatorId",
                table: "SecurityIncidents",
                column: "InvestigatorId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_ReporterId",
                table: "SecurityIncidents",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_Severity",
                table: "SecurityIncidents",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_Severity_CreatedAt",
                table: "SecurityIncidents",
                columns: new[] { "Severity", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_Status",
                table: "SecurityIncidents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_ThreatLevel",
                table: "SecurityIncidents",
                column: "ThreatLevel");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_Type_Status",
                table: "SecurityIncidents",
                columns: new[] { "IncidentType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ThreatAssessments_AssessedById",
                table: "ThreatAssessments",
                column: "AssessedById");

            migrationBuilder.CreateIndex(
                name: "IX_ThreatAssessments_AssessmentDateTime",
                table: "ThreatAssessments",
                column: "AssessmentDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_ThreatAssessments_CurrentThreatLevel",
                table: "ThreatAssessments",
                column: "CurrentThreatLevel");

            migrationBuilder.CreateIndex(
                name: "IX_ThreatAssessments_Incident_DateTime",
                table: "ThreatAssessments",
                columns: new[] { "SecurityIncidentId", "AssessmentDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ThreatAssessments_SecurityIncidentId",
                table: "ThreatAssessments",
                column: "SecurityIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_ThreatIndicators_Active_Confidence",
                table: "ThreatIndicators",
                columns: new[] { "IsActive", "Confidence" });

            migrationBuilder.CreateIndex(
                name: "IX_ThreatIndicators_Confidence",
                table: "ThreatIndicators",
                column: "Confidence");

            migrationBuilder.CreateIndex(
                name: "IX_ThreatIndicators_FirstSeen",
                table: "ThreatIndicators",
                column: "FirstSeen");

            migrationBuilder.CreateIndex(
                name: "IX_ThreatIndicators_IsActive",
                table: "ThreatIndicators",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ThreatIndicators_LastSeen",
                table: "ThreatIndicators",
                column: "LastSeen");

            migrationBuilder.CreateIndex(
                name: "IX_ThreatIndicators_Source",
                table: "ThreatIndicators",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_ThreatIndicators_ThreatType",
                table: "ThreatIndicators",
                column: "ThreatType");

            migrationBuilder.CreateIndex(
                name: "IX_ThreatIndicators_ThreatType_Active",
                table: "ThreatIndicators",
                columns: new[] { "ThreatType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ThreatIndicators_Type_Value",
                table: "ThreatIndicators",
                columns: new[] { "IndicatorType", "IndicatorValue" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleId",
                table: "UserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserId",
                table: "UserRoles");

            migrationBuilder.DropTable(
                name: "SecurityIncidentAttachments");

            migrationBuilder.DropTable(
                name: "SecurityIncidentControls");

            migrationBuilder.DropTable(
                name: "SecurityIncidentIndicators");

            migrationBuilder.DropTable(
                name: "SecurityIncidentInvolvedPersons");

            migrationBuilder.DropTable(
                name: "SecurityIncidentResponses");

            migrationBuilder.DropTable(
                name: "ThreatAssessments");

            migrationBuilder.DropTable(
                name: "SecurityControls");

            migrationBuilder.DropTable(
                name: "ThreatIndicators");

            migrationBuilder.DropTable(
                name: "SecurityIncidents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRoles",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "UserRoles");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                newName: "UserRole");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRole",
                newName: "IX_UserRole_UserId_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRole",
                newName: "IX_UserRole_RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRole",
                table: "UserRole",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Roles_RoleId",
                table: "UserRole",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Users_UserId",
                table: "UserRole",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
