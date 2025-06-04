using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HarmoniHSE360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentReporterFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CorrectiveActions_Users_AssignedToId",
                table: "CorrectiveActions");

            migrationBuilder.AlterColumn<int>(
                name: "ReporterId",
                table: "Incidents",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "EmergencyServicesContacted",
                table: "Incidents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ImmediateActionsTaken",
                table: "Incidents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InjuryType",
                table: "Incidents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MedicalTreatmentProvided",
                table: "Incidents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReporterDepartment",
                table: "Incidents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReporterEmail",
                table: "Incidents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReporterName",
                table: "Incidents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WitnessNames",
                table: "Incidents",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AssignedToId",
                table: "CorrectiveActions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "AssignedToDepartment",
                table: "CorrectiveActions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "CorrectiveActions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_CorrectiveActions_Users_AssignedToId",
                table: "CorrectiveActions",
                column: "AssignedToId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CorrectiveActions_Users_AssignedToId",
                table: "CorrectiveActions");

            migrationBuilder.DropColumn(
                name: "EmergencyServicesContacted",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ImmediateActionsTaken",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "InjuryType",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "MedicalTreatmentProvided",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ReporterDepartment",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ReporterEmail",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ReporterName",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "WitnessNames",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "AssignedToDepartment",
                table: "CorrectiveActions");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "CorrectiveActions");

            migrationBuilder.AlterColumn<int>(
                name: "ReporterId",
                table: "Incidents",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AssignedToId",
                table: "CorrectiveActions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CorrectiveActions_Users_AssignedToId",
                table: "CorrectiveActions",
                column: "AssignedToId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
