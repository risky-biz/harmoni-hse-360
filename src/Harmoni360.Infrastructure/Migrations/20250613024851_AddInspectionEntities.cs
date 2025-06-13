using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InspectionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InspectorId = table.Column<int>(type: "integer", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: true),
                    DepartmentId = table.Column<int>(type: "integer", nullable: true),
                    FacilityId = table.Column<int>(type: "integer", nullable: true),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Recommendations = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    ActualDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inspections_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Inspections_Users_InspectorId",
                        column: x => x.InspectorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InspectionId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsPhoto = table.Column<bool>(type: "boolean", nullable: false),
                    ThumbnailPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionAttachments_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InspectionId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsInternal = table.Column<bool>(type: "boolean", nullable: false),
                    ParentCommentId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionComments_InspectionComments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "InspectionComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionComments_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InspectionComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionFindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InspectionId = table.Column<int>(type: "integer", nullable: false),
                    FindingNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false),
                    RootCause = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImmediateAction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CorrectiveAction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponsiblePersonId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Equipment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Regulation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ClosedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosureNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionFindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionFindings_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InspectionFindings_Users_ResponsiblePersonId",
                        column: x => x.ResponsiblePersonId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InspectionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InspectionId = table.Column<int>(type: "integer", nullable: false),
                    ChecklistItemId = table.Column<int>(type: "integer", nullable: true),
                    Question = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    Response = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ExpectedValue = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MinValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    MaxValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Options = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionItems_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FindingAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FindingId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsPhoto = table.Column<bool>(type: "boolean", nullable: false),
                    ThumbnailPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FindingAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FindingAttachments_InspectionFindings_FindingId",
                        column: x => x.FindingId,
                        principalTable: "InspectionFindings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FindingAttachments_CreatedAt",
                table: "FindingAttachments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FindingAttachments_FindingId",
                table: "FindingAttachments",
                column: "FindingId");

            migrationBuilder.CreateIndex(
                name: "IX_FindingAttachments_IsPhoto",
                table: "FindingAttachments",
                column: "IsPhoto");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionAttachments_Category",
                table: "InspectionAttachments",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionAttachments_CreatedAt",
                table: "InspectionAttachments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionAttachments_InspectionId",
                table: "InspectionAttachments",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionAttachments_IsPhoto",
                table: "InspectionAttachments",
                column: "IsPhoto");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionComments_CreatedAt",
                table: "InspectionComments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionComments_InspectionId",
                table: "InspectionComments",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionComments_InspectionId_CreatedAt",
                table: "InspectionComments",
                columns: new[] { "InspectionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionComments_ParentCommentId",
                table: "InspectionComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionComments_UserId",
                table: "InspectionComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionFindings_DueDate",
                table: "InspectionFindings",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionFindings_FindingNumber",
                table: "InspectionFindings",
                column: "FindingNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionFindings_InspectionId",
                table: "InspectionFindings",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionFindings_ResponsiblePersonId",
                table: "InspectionFindings",
                column: "ResponsiblePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionFindings_Severity",
                table: "InspectionFindings",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionFindings_Status",
                table: "InspectionFindings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionFindings_Status_DueDate",
                table: "InspectionFindings",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionFindings_Type",
                table: "InspectionFindings",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionItems_InspectionId",
                table: "InspectionItems",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionItems_InspectionId_SortOrder",
                table: "InspectionItems",
                columns: new[] { "InspectionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionItems_Status",
                table: "InspectionItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_CreatedAt",
                table: "Inspections",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_DepartmentId",
                table: "Inspections",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_InspectionNumber",
                table: "Inspections",
                column: "InspectionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_InspectorId",
                table: "Inspections",
                column: "InspectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_InspectorId_Status",
                table: "Inspections",
                columns: new[] { "InspectorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_Priority",
                table: "Inspections",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_ScheduledDate",
                table: "Inspections",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_Status",
                table: "Inspections",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_Status_ScheduledDate",
                table: "Inspections",
                columns: new[] { "Status", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_Type",
                table: "Inspections",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FindingAttachments");

            migrationBuilder.DropTable(
                name: "InspectionAttachments");

            migrationBuilder.DropTable(
                name: "InspectionComments");

            migrationBuilder.DropTable(
                name: "InspectionItems");

            migrationBuilder.DropTable(
                name: "InspectionFindings");

            migrationBuilder.DropTable(
                name: "Inspections");
        }
    }
}
