using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkPermitSettingsIdToSafetyVideo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WorkPermitSettingsId",
                table: "WorkPermitSafetyVideos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WorkPermitSettingsId",
                table: "WorkPermitSafetyVideos",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
