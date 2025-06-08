using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EscalationRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    TriggerSeverities = table.Column<string>(type: "json", nullable: false),
                    TriggerStatuses = table.Column<string>(type: "json", nullable: false),
                    TriggerAfterDuration = table.Column<double>(type: "double precision", nullable: true),
                    TriggerDepartments = table.Column<string>(type: "json", nullable: false),
                    TriggerLocations = table.Column<string>(type: "json", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscalationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModulePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Module = table.Column<int>(type: "integer", nullable: false),
                    Permission = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModulePermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PPECategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    RequiresCertification = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresInspection = table.Column<bool>(type: "boolean", nullable: false),
                    InspectionIntervalDays = table.Column<int>(type: "integer", nullable: true),
                    RequiresExpiry = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultExpiryDays = table.Column<int>(type: "integer", nullable: true),
                    ComplianceStandard = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPECategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PPESizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPESizes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PPEStorageLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContactPerson = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Capacity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1000),
                    CurrentStock = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPEStorageLocations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RoleType = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EscalationActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EscalationRuleId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Target = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TemplateId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Parameters = table.Column<string>(type: "json", nullable: false),
                    Delay = table.Column<double>(type: "double precision", nullable: true),
                    Channels = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscalationActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalationActions_EscalationRules_EscalationRuleId",
                        column: x => x.EscalationRuleId,
                        principalTable: "EscalationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonId = table.Column<int>(type: "integer", nullable: false),
                    PersonType = table.Column<int>(type: "integer", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BloodType = table.Column<int>(type: "integer", nullable: true),
                    MedicalNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthRecords_Users_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    IncidentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: true),
                    Longitude = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: true),
                    ReporterName = table.Column<string>(type: "text", nullable: false),
                    ReporterEmail = table.Column<string>(type: "text", nullable: false),
                    ReporterDepartment = table.Column<string>(type: "text", nullable: false),
                    LastResponseAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InjuryType = table.Column<int>(type: "integer", nullable: true),
                    MedicalTreatmentProvided = table.Column<bool>(type: "boolean", nullable: false),
                    EmergencyServicesContacted = table.Column<bool>(type: "boolean", nullable: false),
                    WitnessNames = table.Column<string>(type: "text", nullable: true),
                    ImmediateActionsTaken = table.Column<string>(type: "text", nullable: true),
                    ReporterId = table.Column<int>(type: "integer", nullable: true),
                    InvestigatorId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_Users_InvestigatorId",
                        column: x => x.InvestigatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Incidents_Users_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    SizeId = table.Column<int>(type: "integer", nullable: true),
                    StorageLocationId = table.Column<int>(type: "integer", nullable: true),
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
                        name: "FK_PPEItems_PPESizes_SizeId",
                        column: x => x.SizeId,
                        principalTable: "PPESizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PPEItems_PPEStorageLocations_StorageLocationId",
                        column: x => x.StorageLocationId,
                        principalTable: "PPEStorageLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PPEItems_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RoleModulePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ModulePermissionId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    GrantedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    GrantedByUserId = table.Column<int>(type: "integer", nullable: true),
                    GrantReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleModulePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleModulePermissions_ModulePermissions_ModulePermissionId",
                        column: x => x.ModulePermissionId,
                        principalTable: "ModulePermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleModulePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleModulePermissions_Users_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRole_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HealthRecordId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Relationship = table.Column<int>(type: "integer", nullable: false),
                    CustomRelationship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrimaryPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SecondaryPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsPrimaryContact = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AuthorizedForPickup = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AuthorizedForMedicalDecisions = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ContactOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyContacts_HealthRecords_HealthRecordId",
                        column: x => x.HealthRecordId,
                        principalTable: "HealthRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HealthRecordId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    TreatmentPlan = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DiagnosedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequiresEmergencyAction = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EmergencyInstructions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalConditions_HealthRecords_HealthRecordId",
                        column: x => x.HealthRecordId,
                        principalTable: "HealthRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VaccinationRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HealthRecordId = table.Column<int>(type: "integer", nullable: false),
                    VaccineName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DateAdministered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AdministeredBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AdministrationLocation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ExemptionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaccinationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VaccinationRecords_HealthRecords_HealthRecordId",
                        column: x => x.HealthRecordId,
                        principalTable: "HealthRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CorrectiveActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncidentId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AssignedToDepartment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AssignedToId = table.Column<int>(type: "integer", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    CompletionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrectiveActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorrectiveActions_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CorrectiveActions_Users_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EscalationHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncidentId = table.Column<int>(type: "integer", nullable: false),
                    EscalationRuleId = table.Column<int>(type: "integer", nullable: true),
                    RuleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    ActionTarget = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ActionDetails = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscalationHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalationHistories_EscalationRules_EscalationRuleId",
                        column: x => x.EscalationRuleId,
                        principalTable: "EscalationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EscalationHistories_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthIncidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncidentId = table.Column<int>(type: "integer", nullable: true),
                    HealthRecordId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Symptoms = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    TreatmentProvided = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    TreatmentLocation = table.Column<int>(type: "integer", nullable: false),
                    RequiredHospitalization = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ParentsNotified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ParentNotificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReturnToSchoolDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FollowUpRequired = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TreatedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IncidentDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ResolutionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthIncidents_HealthRecords_HealthRecordId",
                        column: x => x.HealthRecordId,
                        principalTable: "HealthRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthIncidents_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "IncidentAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncidentId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedBy = table.Column<string>(type: "text", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentAttachments_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncidentId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OldValue = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NewValue = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangeDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentAuditLogs_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentInvolvedPersons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncidentId = table.Column<int>(type: "integer", nullable: false),
                    PersonId = table.Column<int>(type: "integer", nullable: false),
                    InvolvementType = table.Column<int>(type: "integer", nullable: false),
                    InjuryDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentInvolvedPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentInvolvedPersons_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IncidentInvolvedPersons_Users_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncidentId = table.Column<int>(type: "integer", nullable: false),
                    RecipientId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecipientType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TemplateId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Metadata = table.Column<string>(type: "json", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationHistories_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_CorrectiveActions_AssignedToId",
                table: "CorrectiveActions",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActions_DueDate",
                table: "CorrectiveActions",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActions_IncidentId",
                table: "CorrectiveActions",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActions_Status",
                table: "CorrectiveActions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_AuthorizedForMedicalDecisions",
                table: "EmergencyContacts",
                column: "AuthorizedForMedicalDecisions");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_AuthorizedForPickup",
                table: "EmergencyContacts",
                column: "AuthorizedForPickup");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_CreatedAt",
                table: "EmergencyContacts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_HealthRecordId",
                table: "EmergencyContacts",
                column: "HealthRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_HealthRecordId_ContactOrder_IsActive",
                table: "EmergencyContacts",
                columns: new[] { "HealthRecordId", "ContactOrder", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_IsActive",
                table: "EmergencyContacts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_IsPrimaryContact",
                table: "EmergencyContacts",
                column: "IsPrimaryContact");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_Relationship",
                table: "EmergencyContacts",
                column: "Relationship");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationActions_EscalationRuleId",
                table: "EscalationActions",
                column: "EscalationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationActions_Type",
                table: "EscalationActions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_EscalationRuleId",
                table: "EscalationHistories",
                column: "EscalationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_ExecutedAt",
                table: "EscalationHistories",
                column: "ExecutedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_IncidentId",
                table: "EscalationHistories",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationHistories_IsSuccessful",
                table: "EscalationHistories",
                column: "IsSuccessful");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationRules_CreatedAt",
                table: "EscalationRules",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationRules_IsActive",
                table: "EscalationRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationRules_Priority",
                table: "EscalationRules",
                column: "Priority");

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
                name: "IX_HealthIncidents_CreatedAt",
                table: "HealthIncidents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HealthIncidents_HealthRecordId",
                table: "HealthIncidents",
                column: "HealthRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthIncidents_IncidentDateTime",
                table: "HealthIncidents",
                column: "IncidentDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_HealthIncidents_IncidentId",
                table: "HealthIncidents",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthIncidents_IsResolved",
                table: "HealthIncidents",
                column: "IsResolved");

            migrationBuilder.CreateIndex(
                name: "IX_HealthIncidents_RequiredHospitalization",
                table: "HealthIncidents",
                column: "RequiredHospitalization");

            migrationBuilder.CreateIndex(
                name: "IX_HealthIncidents_Severity",
                table: "HealthIncidents",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_HealthIncidents_Type",
                table: "HealthIncidents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_HealthIncidents_Type_Severity_IncidentDateTime",
                table: "HealthIncidents",
                columns: new[] { "Type", "Severity", "IncidentDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_HealthRecords_CreatedAt",
                table: "HealthRecords",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HealthRecords_IsActive",
                table: "HealthRecords",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HealthRecords_PersonId",
                table: "HealthRecords",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HealthRecords_PersonType",
                table: "HealthRecords",
                column: "PersonType");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAttachments_IncidentId",
                table: "IncidentAttachments",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAuditLogs_ChangedAt",
                table: "IncidentAuditLogs",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAuditLogs_IncidentId",
                table: "IncidentAuditLogs",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAuditLogs_IncidentId_ChangedAt",
                table: "IncidentAuditLogs",
                columns: new[] { "IncidentId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentInvolvedPersons_IncidentId",
                table: "IncidentInvolvedPersons",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentInvolvedPersons_PersonId",
                table: "IncidentInvolvedPersons",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_CreatedAt",
                table: "Incidents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_IncidentDate",
                table: "Incidents",
                column: "IncidentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_InvestigatorId",
                table: "Incidents",
                column: "InvestigatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_ReporterId",
                table: "Incidents",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_Severity",
                table: "Incidents",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_Status",
                table: "Incidents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalConditions_CreatedAt",
                table: "MedicalConditions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalConditions_HealthRecordId",
                table: "MedicalConditions",
                column: "HealthRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalConditions_IsActive",
                table: "MedicalConditions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalConditions_RequiresEmergencyAction",
                table: "MedicalConditions",
                column: "RequiresEmergencyAction");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalConditions_Severity",
                table: "MedicalConditions",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalConditions_Type",
                table: "MedicalConditions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ModulePermission_IsActive",
                table: "ModulePermissions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ModulePermission_Module",
                table: "ModulePermissions",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_ModulePermission_Module_Permission",
                table: "ModulePermissions",
                columns: new[] { "Module", "Permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_Channel",
                table: "NotificationHistories",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_CreatedAt",
                table: "NotificationHistories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_IncidentId",
                table: "NotificationHistories",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_IncidentId_RecipientId",
                table: "NotificationHistories",
                columns: new[] { "IncidentId", "RecipientId" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_RecipientId",
                table: "NotificationHistories",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistories_Status",
                table: "NotificationHistories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name",
                table: "Permissions",
                column: "Name",
                unique: true);

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
                name: "IX_PPECategories_Code",
                table: "PPECategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PPECategories_IsActive",
                table: "PPECategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PPECategories_Name",
                table: "PPECategories",
                column: "Name");

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
                name: "IX_PPEItems_SizeId",
                table: "PPEItems",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_PPEItems_Status",
                table: "PPEItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PPEItems_Status_CategoryId",
                table: "PPEItems",
                columns: new[] { "Status", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_PPEItems_StorageLocationId",
                table: "PPEItems",
                column: "StorageLocationId");

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

            migrationBuilder.CreateIndex(
                name: "IX_PPESizes_Code",
                table: "PPESizes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PPESizes_IsActive",
                table: "PPESizes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PPESizes_Name",
                table: "PPESizes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PPESizes_SortOrder",
                table: "PPESizes",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_PPEStorageLocations_Code",
                table: "PPEStorageLocations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PPEStorageLocations_IsActive",
                table: "PPEStorageLocations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PPEStorageLocations_Name",
                table: "PPEStorageLocations",
                column: "Name");

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

            migrationBuilder.CreateIndex(
                name: "IX_RoleModulePermission_GrantedByUserId",
                table: "RoleModulePermissions",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleModulePermission_IsActive",
                table: "RoleModulePermissions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RoleModulePermission_ModulePermissionId",
                table: "RoleModulePermissions",
                column: "ModulePermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleModulePermission_Role_ModulePermission",
                table: "RoleModulePermissions",
                columns: new[] { "RoleId", "ModulePermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleModulePermission_RoleId",
                table: "RoleModulePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_IsActive",
                table: "Roles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleType",
                table: "Roles",
                column: "RoleType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId_RoleId",
                table: "UserRole",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeId",
                table: "Users",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_CreatedAt",
                table: "VaccinationRecords",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_DateAdministered",
                table: "VaccinationRecords",
                column: "DateAdministered");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_ExpiryDate",
                table: "VaccinationRecords",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_HealthRecordId",
                table: "VaccinationRecords",
                column: "HealthRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_HealthRecordId_VaccineName_Status",
                table: "VaccinationRecords",
                columns: new[] { "HealthRecordId", "VaccineName", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_IsRequired",
                table: "VaccinationRecords",
                column: "IsRequired");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_Status",
                table: "VaccinationRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecords_VaccineName",
                table: "VaccinationRecords",
                column: "VaccineName");

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
                name: "FK_Hazards_Users_ReporterId",
                table: "Hazards");

            migrationBuilder.DropForeignKey(
                name: "FK_RiskAssessments_Users_ApprovedById",
                table: "RiskAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_RiskAssessments_Users_AssessorId",
                table: "RiskAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_RiskAssessments_Hazards_HazardId",
                table: "RiskAssessments");

            migrationBuilder.DropTable(
                name: "CorrectiveActions");

            migrationBuilder.DropTable(
                name: "EmergencyContacts");

            migrationBuilder.DropTable(
                name: "EscalationActions");

            migrationBuilder.DropTable(
                name: "EscalationHistories");

            migrationBuilder.DropTable(
                name: "HazardAttachments");

            migrationBuilder.DropTable(
                name: "HazardMitigationActions");

            migrationBuilder.DropTable(
                name: "HazardReassessments");

            migrationBuilder.DropTable(
                name: "HealthIncidents");

            migrationBuilder.DropTable(
                name: "IncidentAttachments");

            migrationBuilder.DropTable(
                name: "IncidentAuditLogs");

            migrationBuilder.DropTable(
                name: "IncidentInvolvedPersons");

            migrationBuilder.DropTable(
                name: "MedicalConditions");

            migrationBuilder.DropTable(
                name: "NotificationHistories");

            migrationBuilder.DropTable(
                name: "PPEAssignments");

            migrationBuilder.DropTable(
                name: "PPEComplianceRequirements");

            migrationBuilder.DropTable(
                name: "PPEInspections");

            migrationBuilder.DropTable(
                name: "PPERequestItems");

            migrationBuilder.DropTable(
                name: "RoleModulePermissions");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "VaccinationRecords");

            migrationBuilder.DropTable(
                name: "EscalationRules");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "PPERequests");

            migrationBuilder.DropTable(
                name: "ModulePermissions");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "HealthRecords");

            migrationBuilder.DropTable(
                name: "PPEItems");

            migrationBuilder.DropTable(
                name: "PPECategories");

            migrationBuilder.DropTable(
                name: "PPESizes");

            migrationBuilder.DropTable(
                name: "PPEStorageLocations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Hazards");

            migrationBuilder.DropTable(
                name: "RiskAssessments");
        }
    }
}
