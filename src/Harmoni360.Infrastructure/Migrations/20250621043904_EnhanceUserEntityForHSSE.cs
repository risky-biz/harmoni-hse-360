using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceUserEntityForHSSE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AccountLockedUntil",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CostCenter",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPasswordChange",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "Users",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                defaultValue: "en");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresMFA",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "SupervisorEmployeeId",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkLocation",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ActivityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EntityId = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivityLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Department",
                table: "Users",
                column: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Status",
                table: "Users",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SupervisorEmployeeId",
                table: "Users",
                column: "SupervisorEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_WorkLocation",
                table: "Users",
                column: "WorkLocation");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_ActivityType",
                table: "UserActivityLogs",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_CreatedAt",
                table: "UserActivityLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivityLogs_UserId_CreatedAt",
                table: "UserActivityLogs",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_Users_Department",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Status",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SupervisorEmployeeId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_WorkLocation",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AccountLockedUntil",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CostCenter",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastPasswordChange",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RequiresMFA",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SupervisorEmployeeId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WorkLocation",
                table: "Users");
        }
    }
}
