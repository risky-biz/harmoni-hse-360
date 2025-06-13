using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationManagementEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Incidents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Incidents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Incidents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "CorrectiveActions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HeadOfDepartment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Contact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    RequiresImmediateAction = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Building = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Floor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Room = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", precision: 10, scale: 8, nullable: true),
                    Longitude = table.Column<double>(type: "double precision", precision: 11, scale: 8, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsHighRisk = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentLocations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_CategoryId",
                table: "Incidents",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_DepartmentId",
                table: "Incidents",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_LocationId",
                table: "Incidents",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActions_DepartmentId",
                table: "CorrectiveActions",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Code",
                table: "Departments",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_IsActive",
                table: "Departments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCategories_Code",
                table: "IncidentCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCategories_IsActive",
                table: "IncidentCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCategories_Name",
                table: "IncidentCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentLocations_Building",
                table: "IncidentLocations",
                column: "Building");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentLocations_Code",
                table: "IncidentLocations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentLocations_IsActive",
                table: "IncidentLocations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentLocations_Name",
                table: "IncidentLocations",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_CorrectiveActions_Departments_DepartmentId",
                table: "CorrectiveActions",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Departments_DepartmentId",
                table: "Incidents",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_IncidentCategories_CategoryId",
                table: "Incidents",
                column: "CategoryId",
                principalTable: "IncidentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_IncidentLocations_LocationId",
                table: "Incidents",
                column: "LocationId",
                principalTable: "IncidentLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CorrectiveActions_Departments_DepartmentId",
                table: "CorrectiveActions");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Departments_DepartmentId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_IncidentCategories_CategoryId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_IncidentLocations_LocationId",
                table: "Incidents");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "IncidentCategories");

            migrationBuilder.DropTable(
                name: "IncidentLocations");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_CategoryId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_DepartmentId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_LocationId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_CorrectiveActions_DepartmentId",
                table: "CorrectiveActions");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "CorrectiveActions");
        }
    }
}
