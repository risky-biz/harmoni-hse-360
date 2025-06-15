using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Audits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuditNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuditorId = table.Column<int>(type: "integer", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: true),
                    DepartmentId = table.Column<int>(type: "integer", nullable: true),
                    FacilityId = table.Column<int>(type: "integer", nullable: true),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Recommendations = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OverallScore = table.Column<int>(type: "integer", nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    ActualDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    StandardsApplied = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsRegulatory = table.Column<bool>(type: "boolean", nullable: false),
                    RegulatoryReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ScorePercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    TotalPossiblePoints = table.Column<int>(type: "integer", nullable: true),
                    AchievedPoints = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Audits_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Audits_Users_AuditorId",
                        column: x => x.AuditorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuditId = table.Column<int>(type: "integer", nullable: false),
                    ItemNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ExpectedResult = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActualResult = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsCompliant = table.Column<bool>(type: "boolean", nullable: true),
                    MaxPoints = table.Column<int>(type: "integer", nullable: true),
                    ActualPoints = table.Column<int>(type: "integer", nullable: true),
                    AssessedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AssessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Evidence = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CorrectiveAction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponsiblePersonId = table.Column<int>(type: "integer", nullable: true),
                    ValidationCriteria = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AcceptanceCriteria = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditItems_Audits_AuditId",
                        column: x => x.AuditId,
                        principalTable: "Audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditItems_Users_ResponsiblePersonId",
                        column: x => x.ResponsiblePersonId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AuditAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuditId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UploadedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AttachmentType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsEvidence = table.Column<bool>(type: "boolean", nullable: false),
                    AuditItemId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditAttachments_AuditItems_AuditItemId",
                        column: x => x.AuditItemId,
                        principalTable: "AuditItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditAttachments_Audits_AuditId",
                        column: x => x.AuditId,
                        principalTable: "Audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditFindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuditId = table.Column<int>(type: "integer", nullable: false),
                    FindingNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Equipment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Standard = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Regulation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AuditItemId = table.Column<int>(type: "integer", nullable: true),
                    RootCause = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImmediateAction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CorrectiveAction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PreventiveAction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponsiblePersonId = table.Column<int>(type: "integer", nullable: true),
                    ResponsiblePersonName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClosedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosureNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ClosedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VerificationMethod = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RequiresVerification = table.Column<bool>(type: "boolean", nullable: false),
                    VerificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ActualCost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BusinessImpact = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditFindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditFindings_AuditItems_AuditItemId",
                        column: x => x.AuditItemId,
                        principalTable: "AuditItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AuditFindings_Audits_AuditId",
                        column: x => x.AuditId,
                        principalTable: "Audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditFindings_Users_ResponsiblePersonId",
                        column: x => x.ResponsiblePersonId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AuditComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuditId = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CommentedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CommentedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditItemId = table.Column<int>(type: "integer", nullable: true),
                    AuditFindingId = table.Column<int>(type: "integer", nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsInternal = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditComments_AuditFindings_AuditFindingId",
                        column: x => x.AuditFindingId,
                        principalTable: "AuditFindings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditComments_AuditItems_AuditItemId",
                        column: x => x.AuditItemId,
                        principalTable: "AuditItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditComments_Audits_AuditId",
                        column: x => x.AuditId,
                        principalTable: "Audits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditFindingAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuditFindingId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UploadedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AttachmentType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsEvidence = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditFindingAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditFindingAttachments_AuditFindings_AuditFindingId",
                        column: x => x.AuditFindingId,
                        principalTable: "AuditFindings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditAttachments_AttachmentType",
                table: "AuditAttachments",
                column: "AttachmentType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditAttachments_AuditId",
                table: "AuditAttachments",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditAttachments_AuditId_Category",
                table: "AuditAttachments",
                columns: new[] { "AuditId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditAttachments_AuditId_IsEvidence",
                table: "AuditAttachments",
                columns: new[] { "AuditId", "IsEvidence" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditAttachments_AuditItemId",
                table: "AuditAttachments",
                column: "AuditItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditAttachments_ContentType",
                table: "AuditAttachments",
                column: "ContentType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditAttachments_FileName",
                table: "AuditAttachments",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditAttachments_IsEvidence",
                table: "AuditAttachments",
                column: "IsEvidence");

            migrationBuilder.CreateIndex(
                name: "IX_AuditAttachments_UploadedAt",
                table: "AuditAttachments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditComments_AuditFindingId",
                table: "AuditComments",
                column: "AuditFindingId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditComments_AuditId",
                table: "AuditComments",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditComments_AuditId_Category",
                table: "AuditComments",
                columns: new[] { "AuditId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditComments_AuditId_CommentedAt",
                table: "AuditComments",
                columns: new[] { "AuditId", "CommentedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditComments_AuditItemId",
                table: "AuditComments",
                column: "AuditItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditComments_CommentedAt",
                table: "AuditComments",
                column: "CommentedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditComments_IsInternal",
                table: "AuditComments",
                column: "IsInternal");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindingAttachments_AttachmentType",
                table: "AuditFindingAttachments",
                column: "AttachmentType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindingAttachments_AuditFindingId",
                table: "AuditFindingAttachments",
                column: "AuditFindingId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindingAttachments_AuditFindingId_AttachmentType",
                table: "AuditFindingAttachments",
                columns: new[] { "AuditFindingId", "AttachmentType" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindingAttachments_AuditFindingId_IsEvidence",
                table: "AuditFindingAttachments",
                columns: new[] { "AuditFindingId", "IsEvidence" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindingAttachments_ContentType",
                table: "AuditFindingAttachments",
                column: "ContentType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindingAttachments_FileName",
                table: "AuditFindingAttachments",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindingAttachments_IsEvidence",
                table: "AuditFindingAttachments",
                column: "IsEvidence");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindingAttachments_UploadedAt",
                table: "AuditFindingAttachments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindings_AuditId",
                table: "AuditFindings",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindings_AuditId_Status",
                table: "AuditFindings",
                columns: new[] { "AuditId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindings_AuditItemId",
                table: "AuditFindings",
                column: "AuditItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindings_DueDate",
                table: "AuditFindings",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindings_FindingNumber",
                table: "AuditFindings",
                column: "FindingNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindings_RequiresVerification",
                table: "AuditFindings",
                column: "RequiresVerification");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindings_ResponsiblePersonId",
                table: "AuditFindings",
                column: "ResponsiblePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindings_RiskLevel",
                table: "AuditFindings",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindings_Severity",
                table: "AuditFindings",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindings_Status",
                table: "AuditFindings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindings_Status_Severity",
                table: "AuditFindings",
                columns: new[] { "Status", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindings_Type",
                table: "AuditFindings",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindings_Type_Severity",
                table: "AuditFindings",
                columns: new[] { "Type", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditFindings_VerificationDate",
                table: "AuditFindings",
                column: "VerificationDate");

            migrationBuilder.CreateIndex(
                name: "IX_AuditItems_AuditId",
                table: "AuditItems",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditItems_AuditId_SortOrder",
                table: "AuditItems",
                columns: new[] { "AuditId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditItems_AuditId_Status",
                table: "AuditItems",
                columns: new[] { "AuditId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditItems_Category",
                table: "AuditItems",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_AuditItems_DueDate",
                table: "AuditItems",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_AuditItems_IsRequired",
                table: "AuditItems",
                column: "IsRequired");

            migrationBuilder.CreateIndex(
                name: "IX_AuditItems_ItemNumber",
                table: "AuditItems",
                column: "ItemNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AuditItems_ResponsiblePersonId",
                table: "AuditItems",
                column: "ResponsiblePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditItems_SortOrder",
                table: "AuditItems",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_AuditItems_Status",
                table: "AuditItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AuditItems_Status_IsRequired",
                table: "AuditItems",
                columns: new[] { "Status", "IsRequired" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditItems_Type",
                table: "AuditItems",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_AuditNumber",
                table: "Audits",
                column: "AuditNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Audits_AuditorId",
                table: "Audits",
                column: "AuditorId");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_AuditorId_Status",
                table: "Audits",
                columns: new[] { "AuditorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Audits_Category",
                table: "Audits",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_CreatedAt",
                table: "Audits",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_DepartmentId",
                table: "Audits",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_Priority",
                table: "Audits",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_RiskLevel",
                table: "Audits",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_ScheduledDate",
                table: "Audits",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_Status",
                table: "Audits",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_Status_ScheduledDate",
                table: "Audits",
                columns: new[] { "Status", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Audits_Type",
                table: "Audits",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_Type_Status",
                table: "Audits",
                columns: new[] { "Type", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditAttachments");

            migrationBuilder.DropTable(
                name: "AuditComments");

            migrationBuilder.DropTable(
                name: "AuditFindingAttachments");

            migrationBuilder.DropTable(
                name: "AuditFindings");

            migrationBuilder.DropTable(
                name: "AuditItems");

            migrationBuilder.DropTable(
                name: "Audits");
        }
    }
}
