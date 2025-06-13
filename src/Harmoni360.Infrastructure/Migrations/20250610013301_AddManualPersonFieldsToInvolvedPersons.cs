using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManualPersonFieldsToInvolvedPersons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncidentInvolvedPersons_Users_PersonId",
                table: "IncidentInvolvedPersons");

            migrationBuilder.DropIndex(
                name: "IX_IncidentInvolvedPersons_IncidentId",
                table: "IncidentInvolvedPersons");

            migrationBuilder.AlterColumn<int>(
                name: "PersonId",
                table: "IncidentInvolvedPersons",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "InjuryDescription",
                table: "IncidentInvolvedPersons",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManualPersonEmail",
                table: "IncidentInvolvedPersons",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManualPersonName",
                table: "IncidentInvolvedPersons",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentInvolvedPersons_IncidentId_ManualPersonName",
                table: "IncidentInvolvedPersons",
                columns: new[] { "IncidentId", "ManualPersonName" });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentInvolvedPersons_IncidentId_PersonId",
                table: "IncidentInvolvedPersons",
                columns: new[] { "IncidentId", "PersonId" },
                unique: true,
                filter: "\"PersonId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentInvolvedPersons_Users_PersonId",
                table: "IncidentInvolvedPersons",
                column: "PersonId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncidentInvolvedPersons_Users_PersonId",
                table: "IncidentInvolvedPersons");

            migrationBuilder.DropIndex(
                name: "IX_IncidentInvolvedPersons_IncidentId_ManualPersonName",
                table: "IncidentInvolvedPersons");

            migrationBuilder.DropIndex(
                name: "IX_IncidentInvolvedPersons_IncidentId_PersonId",
                table: "IncidentInvolvedPersons");

            migrationBuilder.DropColumn(
                name: "ManualPersonEmail",
                table: "IncidentInvolvedPersons");

            migrationBuilder.DropColumn(
                name: "ManualPersonName",
                table: "IncidentInvolvedPersons");

            migrationBuilder.AlterColumn<int>(
                name: "PersonId",
                table: "IncidentInvolvedPersons",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InjuryDescription",
                table: "IncidentInvolvedPersons",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentInvolvedPersons_IncidentId",
                table: "IncidentInvolvedPersons",
                column: "IncidentId");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentInvolvedPersons_Users_PersonId",
                table: "IncidentInvolvedPersons",
                column: "PersonId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
