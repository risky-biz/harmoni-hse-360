using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonEntityAndUpdateHealthRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthRecords_Users_PersonId",
                table: "HealthRecords");

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Position = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PersonType = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Persons_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Email",
                table: "Persons",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_EmployeeId_IsActive",
                table: "Persons",
                columns: new[] { "EmployeeId", "IsActive" },
                filter: "\"EmployeeId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_PersonType_IsActive",
                table: "Persons",
                columns: new[] { "PersonType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Persons_UserId",
                table: "Persons",
                column: "UserId");

            // Migrate existing health records to use persons
            // First, create person records for all existing health records
            migrationBuilder.Sql(@"
                INSERT INTO ""Persons"" (""Name"", ""Email"", ""PhoneNumber"", ""Department"", ""Position"", ""PersonType"", ""EmployeeId"", ""IsActive"", ""UserId"", ""CreatedAt"", ""CreatedBy"")
                SELECT 
                    u.""Name"",
                    u.""Email"",
                    u.""PhoneNumber"",
                    u.""Department"",
                    u.""Position"",
                    hr.""PersonType"",
                    u.""EmployeeId"",
                    hr.""IsActive"",
                    u.""Id"",
                    hr.""CreatedAt"",
                    hr.""CreatedBy""
                FROM ""HealthRecords"" hr
                INNER JOIN ""Users"" u ON hr.""PersonId"" = u.""Id""
            ");

            // Update HealthRecords to reference the new Person records
            migrationBuilder.Sql(@"
                UPDATE ""HealthRecords"" 
                SET ""PersonId"" = p.""Id""
                FROM ""Persons"" p 
                WHERE ""HealthRecords"".""PersonId"" = p.""UserId""
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthRecords_Persons_PersonId",
                table: "HealthRecords",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthRecords_Persons_PersonId",
                table: "HealthRecords");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthRecords_Users_PersonId",
                table: "HealthRecords",
                column: "PersonId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
