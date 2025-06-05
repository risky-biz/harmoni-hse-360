using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HarmoniHSE360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPPEManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PPECategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    RequiresCertification = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresInspection = table.Column<bool>(type: "boolean", nullable: false),
                    InspectionIntervalDays = table.Column<int>(type: "integer", nullable: true),
                    RequiresExpiry = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultExpiryDays = table.Column<int>(type: "integer", nullable: true),
                    ComplianceStandard = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPECategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PPEComplianceRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    RiskAssessmentReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ComplianceNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MinimumQuantity = table.Column<int>(type: "integer", nullable: true),
                    ReplacementIntervalDays = table.Column<int>(type: "integer", nullable: true),
                    RequiresTraining = table.Column<bool>(type: "boolean", nullable: false),
                    TrainingRequirements = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPEComplianceRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PPEComplianceRequirements_PPECategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "PPECategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PPEComplianceRequirements_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PPEItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Size = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Condition = table.Column<string>(type: "text", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AssignedToId = table.Column<int>(type: "integer", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CertificationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CertifyingBody = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CertificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CertificationExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CertificationStandard = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MaintenanceIntervalDays = table.Column<int>(type: "integer", nullable: true),
                    LastMaintenanceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaintenanceInstructions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPEItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PPEItems_PPECategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "PPECategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PPEItems_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PPEAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PPEItemId = table.Column<int>(type: "integer", nullable: false),
                    AssignedToId = table.Column<int>(type: "integer", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReturnedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ReturnedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Purpose = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ReturnNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPEAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PPEAssignments_PPEItems_PPEItemId",
                        column: x => x.PPEItemId,
                        principalTable: "PPEItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PPEAssignments_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PPEInspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PPEItemId = table.Column<int>(type: "integer", nullable: false),
                    InspectorId = table.Column<int>(type: "integer", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextInspectionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Result = table.Column<string>(type: "text", nullable: false),
                    Findings = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CorrectiveActions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RecommendedCondition = table.Column<string>(type: "text", nullable: true),
                    RequiresMaintenance = table.Column<bool>(type: "boolean", nullable: false),
                    MaintenanceNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PhotoPaths = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPEInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PPEInspections_PPEItems_PPEItemId",
                        column: x => x.PPEItemId,
                        principalTable: "PPEItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PPEInspections_Users_InspectorId",
                        column: x => x.InspectorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PPERequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequesterId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Justification = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequiredDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewerId = table.Column<int>(type: "integer", nullable: true),
                    ReviewedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FulfilledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FulfilledBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FulfilledPPEItemId = table.Column<int>(type: "integer", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPERequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PPERequests_PPECategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "PPECategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PPERequests_PPEItems_FulfilledPPEItemId",
                        column: x => x.FulfilledPPEItemId,
                        principalTable: "PPEItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PPERequests_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PPERequests_Users_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PPERequestItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestId = table.Column<int>(type: "integer", nullable: false),
                    ItemDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Size = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    SpecialRequirements = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPERequestItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PPERequestItems_PPERequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "PPERequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PPEAssignments_AssignedDate",
                table: "PPEAssignments",
                column: "AssignedDate");

            migrationBuilder.CreateIndex(
                name: "IX_PPEAssignments_AssignedToId",
                table: "PPEAssignments",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_PPEAssignments_PPEItemId",
                table: "PPEAssignments",
                column: "PPEItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PPEAssignments_PPEItemId_Status",
                table: "PPEAssignments",
                columns: new[] { "PPEItemId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PPEAssignments_ReturnedDate",
                table: "PPEAssignments",
                column: "ReturnedDate");

            migrationBuilder.CreateIndex(
                name: "IX_PPEAssignments_Status",
                table: "PPEAssignments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PPECategories_IsActive",
                table: "PPECategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PPECategories_Name",
                table: "PPECategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PPECategories_Type",
                table: "PPECategories",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_PPEComplianceRequirements_CategoryId",
                table: "PPEComplianceRequirements",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PPEComplianceRequirements_IsActive",
                table: "PPEComplianceRequirements",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PPEComplianceRequirements_IsMandatory",
                table: "PPEComplianceRequirements",
                column: "IsMandatory");

            migrationBuilder.CreateIndex(
                name: "IX_PPEComplianceRequirements_RoleId",
                table: "PPEComplianceRequirements",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PPEComplianceRequirements_RoleId_CategoryId",
                table: "PPEComplianceRequirements",
                columns: new[] { "RoleId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PPEInspections_InspectionDate",
                table: "PPEInspections",
                column: "InspectionDate");

            migrationBuilder.CreateIndex(
                name: "IX_PPEInspections_InspectorId",
                table: "PPEInspections",
                column: "InspectorId");

            migrationBuilder.CreateIndex(
                name: "IX_PPEInspections_NextInspectionDate",
                table: "PPEInspections",
                column: "NextInspectionDate");

            migrationBuilder.CreateIndex(
                name: "IX_PPEInspections_PPEItemId",
                table: "PPEInspections",
                column: "PPEItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PPEInspections_PPEItemId_InspectionDate",
                table: "PPEInspections",
                columns: new[] { "PPEItemId", "InspectionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PPEInspections_Result",
                table: "PPEInspections",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_PPEItems_AssignedToId",
                table: "PPEItems",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_PPEItems_CategoryId",
                table: "PPEItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PPEItems_Condition",
                table: "PPEItems",
                column: "Condition");

            migrationBuilder.CreateIndex(
                name: "IX_PPEItems_CreatedAt",
                table: "PPEItems",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PPEItems_ExpiryDate",
                table: "PPEItems",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_PPEItems_ItemCode",
                table: "PPEItems",
                column: "ItemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PPEItems_Status",
                table: "PPEItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PPEItems_Status_CategoryId",
                table: "PPEItems",
                columns: new[] { "Status", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_PPERequestItems_RequestId",
                table: "PPERequestItems",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PPERequests_CategoryId",
                table: "PPERequests",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PPERequests_FulfilledPPEItemId",
                table: "PPERequests",
                column: "FulfilledPPEItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PPERequests_Priority",
                table: "PPERequests",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_PPERequests_RequestDate",
                table: "PPERequests",
                column: "RequestDate");

            migrationBuilder.CreateIndex(
                name: "IX_PPERequests_RequesterId",
                table: "PPERequests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_PPERequests_RequestNumber",
                table: "PPERequests",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PPERequests_RequiredDate",
                table: "PPERequests",
                column: "RequiredDate");

            migrationBuilder.CreateIndex(
                name: "IX_PPERequests_ReviewerId",
                table: "PPERequests",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_PPERequests_Status",
                table: "PPERequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PPERequests_Status_Priority",
                table: "PPERequests",
                columns: new[] { "Status", "Priority" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PPEAssignments");

            migrationBuilder.DropTable(
                name: "PPEComplianceRequirements");

            migrationBuilder.DropTable(
                name: "PPEInspections");

            migrationBuilder.DropTable(
                name: "PPERequestItems");

            migrationBuilder.DropTable(
                name: "PPERequests");

            migrationBuilder.DropTable(
                name: "PPEItems");

            migrationBuilder.DropTable(
                name: "PPECategories");
        }
    }
}
