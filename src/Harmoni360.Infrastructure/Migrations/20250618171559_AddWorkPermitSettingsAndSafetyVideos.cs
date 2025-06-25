using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkPermitSettingsAndSafetyVideos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkPermitSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequireSafetyInduction = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EnableFormValidation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AllowAttachments = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MaxAttachmentSizeMB = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    FormInstructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkPermitSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkPermitSafetyVideos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ThumbnailPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Resolution = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Bitrate = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WorkPermitSettingsId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkPermitSafetyVideos", x => x.Id);
                    table.CheckConstraint("CK_WorkPermitSafetyVideos_Duration_Positive", "\"Duration\" > '00:00:00'");
                    table.CheckConstraint("CK_WorkPermitSafetyVideos_FileSize_Positive", "\"FileSize\" > 0");
                    table.ForeignKey(
                        name: "FK_WorkPermitSafetyVideos_WorkPermitSettings_WorkPermitSetting~",
                        column: x => x.WorkPermitSettingsId,
                        principalTable: "WorkPermitSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitSafetyVideos_ContentType",
                table: "WorkPermitSafetyVideos",
                column: "ContentType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitSafetyVideos_CreatedAt",
                table: "WorkPermitSafetyVideos",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitSafetyVideos_FileName",
                table: "WorkPermitSafetyVideos",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitSafetyVideos_IsActive",
                table: "WorkPermitSafetyVideos",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitSafetyVideos_IsActive_CreatedAt",
                table: "WorkPermitSafetyVideos",
                columns: new[] { "IsActive", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitSafetyVideos_WorkPermitSettingsId",
                table: "WorkPermitSafetyVideos",
                column: "WorkPermitSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitSettings_CreatedAt",
                table: "WorkPermitSettings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkPermitSettings_IsActive",
                table: "WorkPermitSettings",
                column: "IsActive",
                unique: true,
                filter: "\"IsActive\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkPermitSafetyVideos");

            migrationBuilder.DropTable(
                name: "WorkPermitSettings");
        }
    }
}
