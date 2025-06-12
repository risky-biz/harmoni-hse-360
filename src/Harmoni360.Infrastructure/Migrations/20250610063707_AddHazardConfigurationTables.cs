using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHazardConfigurationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Hazards_Category",
                table: "Hazards");

            migrationBuilder.DropIndex(
                name: "IX_Hazards_Category_Status",
                table: "Hazards");

            migrationBuilder.DropIndex(
                name: "IX_Hazards_Type",
                table: "Hazards");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Hazards");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Hazards");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Hazards",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Hazards",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HazardCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HazardCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HazardTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    RiskMultiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    RequiresPermit = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HazardTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HazardTypes_HazardCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "HazardCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_CategoryId",
                table: "Hazards",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_CategoryId_Status",
                table: "Hazards",
                columns: new[] { "CategoryId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_TypeId",
                table: "Hazards",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HazardCategories_Code",
                table: "HazardCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HazardCategories_IsActive",
                table: "HazardCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HazardCategories_Name",
                table: "HazardCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_HazardCategories_RiskLevel",
                table: "HazardCategories",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_HazardTypes_CategoryId",
                table: "HazardTypes",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HazardTypes_Code",
                table: "HazardTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HazardTypes_IsActive",
                table: "HazardTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HazardTypes_Name",
                table: "HazardTypes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_HazardTypes_RequiresPermit",
                table: "HazardTypes",
                column: "RequiresPermit");

            migrationBuilder.AddForeignKey(
                name: "FK_Hazards_HazardCategories_CategoryId",
                table: "Hazards",
                column: "CategoryId",
                principalTable: "HazardCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Hazards_HazardTypes_TypeId",
                table: "Hazards",
                column: "TypeId",
                principalTable: "HazardTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hazards_HazardCategories_CategoryId",
                table: "Hazards");

            migrationBuilder.DropForeignKey(
                name: "FK_Hazards_HazardTypes_TypeId",
                table: "Hazards");

            migrationBuilder.DropTable(
                name: "HazardTypes");

            migrationBuilder.DropTable(
                name: "HazardCategories");

            migrationBuilder.DropIndex(
                name: "IX_Hazards_CategoryId",
                table: "Hazards");

            migrationBuilder.DropIndex(
                name: "IX_Hazards_CategoryId_Status",
                table: "Hazards");

            migrationBuilder.DropIndex(
                name: "IX_Hazards_TypeId",
                table: "Hazards");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Hazards");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Hazards");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Hazards",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Hazards",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_Category",
                table: "Hazards",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_Category_Status",
                table: "Hazards",
                columns: new[] { "Category", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Hazards_Type",
                table: "Hazards",
                column: "Type");
        }
    }
}
