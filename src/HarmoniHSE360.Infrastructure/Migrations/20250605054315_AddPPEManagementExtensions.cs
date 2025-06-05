using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HarmoniHSE360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPPEManagementExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PPECategories_Name",
                table: "PPECategories");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "PPEItems");

            migrationBuilder.AddColumn<int>(
                name: "SizeId",
                table: "PPEItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StorageLocationId",
                table: "PPEItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PPECategories",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PPECategories",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PPECategories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PPECategories",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "PPECategories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "PPECategories",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PPESizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPESizes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PPEStorageLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContactPerson = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Capacity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1000),
                    CurrentStock = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PPEStorageLocations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PPEItems_SizeId",
                table: "PPEItems",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_PPEItems_StorageLocationId",
                table: "PPEItems",
                column: "StorageLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PPECategories_Code",
                table: "PPECategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PPECategories_Name",
                table: "PPECategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PPESizes_Code",
                table: "PPESizes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PPESizes_IsActive",
                table: "PPESizes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PPESizes_Name",
                table: "PPESizes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PPESizes_SortOrder",
                table: "PPESizes",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_PPEStorageLocations_Code",
                table: "PPEStorageLocations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PPEStorageLocations_IsActive",
                table: "PPEStorageLocations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PPEStorageLocations_Name",
                table: "PPEStorageLocations",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_PPEItems_PPESizes_SizeId",
                table: "PPEItems",
                column: "SizeId",
                principalTable: "PPESizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PPEItems_PPEStorageLocations_StorageLocationId",
                table: "PPEItems",
                column: "StorageLocationId",
                principalTable: "PPEStorageLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PPEItems_PPESizes_SizeId",
                table: "PPEItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PPEItems_PPEStorageLocations_StorageLocationId",
                table: "PPEItems");

            migrationBuilder.DropTable(
                name: "PPESizes");

            migrationBuilder.DropTable(
                name: "PPEStorageLocations");

            migrationBuilder.DropIndex(
                name: "IX_PPEItems_SizeId",
                table: "PPEItems");

            migrationBuilder.DropIndex(
                name: "IX_PPEItems_StorageLocationId",
                table: "PPEItems");

            migrationBuilder.DropIndex(
                name: "IX_PPECategories_Code",
                table: "PPECategories");

            migrationBuilder.DropIndex(
                name: "IX_PPECategories_Name",
                table: "PPECategories");

            migrationBuilder.DropColumn(
                name: "SizeId",
                table: "PPEItems");

            migrationBuilder.DropColumn(
                name: "StorageLocationId",
                table: "PPEItems");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "PPECategories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PPECategories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PPECategories");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "PPECategories");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "PPECategories");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "PPEItems",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PPECategories",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_PPECategories_Name",
                table: "PPECategories",
                column: "Name",
                unique: true);
        }
    }
}
