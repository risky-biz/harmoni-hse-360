using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLicenseManagementSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Licenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LicenseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IssuingAuthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IssuingAuthorityContact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IssuedLocation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RenewalRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RenewalPeriodDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 90),
                    NextRenewalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AutoRenewal = table.Column<bool>(type: "boolean", nullable: false),
                    RenewalProcedure = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RegulatoryFramework = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApplicableRegulations = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ComplianceStandards = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Scope = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CoverageAreas = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Restrictions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Conditions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    HolderId = table.Column<int>(type: "integer", nullable: false),
                    HolderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LicenseFee = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsCriticalLicense = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RequiresInsurance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RequiredInsuranceAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    SubmittedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActivatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SuspendedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StatusNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LicenseAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LicenseId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UploadedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AttachmentType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenseAttachments_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LicenseAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LicenseId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    ActionDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OldValues = table.Column<string>(type: "text", nullable: false),
                    NewValues = table.Column<string>(type: "text", nullable: false),
                    PerformedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Comments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenseAuditLogs_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LicenseConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LicenseId = table.Column<int>(type: "integer", nullable: false),
                    ConditionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ComplianceEvidence = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ComplianceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ResponsiblePerson = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenseConditions_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LicenseRenewals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LicenseId = table.Column<int>(type: "integer", nullable: false),
                    RenewalNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NewExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RenewalNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RenewalFee = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DocumentsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    InspectionRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    InspectionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseRenewals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenseRenewals_Licenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "Licenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LicenseAttachments_LicenseId",
                table: "LicenseAttachments",
                column: "LicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseAttachments_Type",
                table: "LicenseAttachments",
                column: "AttachmentType");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseAttachments_UploadedAt",
                table: "LicenseAttachments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseAuditLogs_Action",
                table: "LicenseAuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseAuditLogs_LicenseId",
                table: "LicenseAuditLogs",
                column: "LicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseAuditLogs_PerformedAt",
                table: "LicenseAuditLogs",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseAuditLogs_PerformedBy",
                table: "LicenseAuditLogs",
                column: "PerformedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseConditions_DueDate",
                table: "LicenseConditions",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseConditions_IsMandatory",
                table: "LicenseConditions",
                column: "IsMandatory");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseConditions_LicenseId",
                table: "LicenseConditions",
                column: "LicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseConditions_Status",
                table: "LicenseConditions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseRenewals_ApplicationDate",
                table: "LicenseRenewals",
                column: "ApplicationDate");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseRenewals_LicenseId",
                table: "LicenseRenewals",
                column: "LicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseRenewals_NewExpiryDate",
                table: "LicenseRenewals",
                column: "NewExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseRenewals_RenewalNumber",
                table: "LicenseRenewals",
                column: "RenewalNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LicenseRenewals_Status",
                table: "LicenseRenewals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_ExpiryDate",
                table: "Licenses",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_Holder",
                table: "Licenses",
                column: "HolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_IssuingAuthority",
                table: "Licenses",
                column: "IssuingAuthority");

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_LicenseNumber",
                table: "Licenses",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_Number_Authority",
                table: "Licenses",
                columns: new[] { "LicenseNumber", "IssuingAuthority" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_Status",
                table: "Licenses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_Type",
                table: "Licenses",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LicenseAttachments");

            migrationBuilder.DropTable(
                name: "LicenseAuditLogs");

            migrationBuilder.DropTable(
                name: "LicenseConditions");

            migrationBuilder.DropTable(
                name: "LicenseRenewals");

            migrationBuilder.DropTable(
                name: "Licenses");
        }
    }
}
