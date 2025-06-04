using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HarmoniHSE360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLastResponseAtToIncident : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastResponseAt",
                table: "Incidents",
                type: "timestamp with time zone",
                nullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EscalationActions");

            migrationBuilder.DropTable(
                name: "EscalationHistories");

            migrationBuilder.DropTable(
                name: "NotificationHistories");

            migrationBuilder.DropTable(
                name: "EscalationRules");

            migrationBuilder.DropColumn(
                name: "LastResponseAt",
                table: "Incidents");
        }
    }
}
