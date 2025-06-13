using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityIncidentManagementSystemUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SecurityAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Resource = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ActionTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    RelatedIncidentId = table.Column<int>(type: "integer", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: false),
                    IsSecurityCritical = table.Column<bool>(type: "boolean", nullable: false),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityAuditLogs_SecurityIncidents_RelatedIncidentId",
                        column: x => x.RelatedIncidentId,
                        principalTable: "SecurityIncidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SecurityAuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_Action",
                table: "SecurityAuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_Action_Timestamp",
                table: "SecurityAuditLogs",
                columns: new[] { "Action", "ActionTimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_ActionTimestamp",
                table: "SecurityAuditLogs",
                column: "ActionTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_Category",
                table: "SecurityAuditLogs",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_Category_Timestamp",
                table: "SecurityAuditLogs",
                columns: new[] { "Category", "ActionTimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_Critical_Timestamp",
                table: "SecurityAuditLogs",
                columns: new[] { "IsSecurityCritical", "ActionTimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_Incident_Timestamp",
                table: "SecurityAuditLogs",
                columns: new[] { "RelatedIncidentId", "ActionTimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_IpAddress",
                table: "SecurityAuditLogs",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_IsSecurityCritical",
                table: "SecurityAuditLogs",
                column: "IsSecurityCritical");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_RelatedIncidentId",
                table: "SecurityAuditLogs",
                column: "RelatedIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_SessionId",
                table: "SecurityAuditLogs",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_Severity",
                table: "SecurityAuditLogs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_User_Timestamp",
                table: "SecurityAuditLogs",
                columns: new[] { "UserId", "ActionTimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_UserId",
                table: "SecurityAuditLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecurityAuditLogs");
        }
    }
}
