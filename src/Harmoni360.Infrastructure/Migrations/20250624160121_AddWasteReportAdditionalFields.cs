using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harmoni360.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWasteReportAdditionalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractorName",
                table: "WasteReports",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DisposalCost",
                table: "WasteReports",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisposalDate",
                table: "WasteReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisposalMethod",
                table: "WasteReports",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisposedBy",
                table: "WasteReports",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedQuantity",
                table: "WasteReports",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManifestNumber",
                table: "WasteReports",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "WasteReports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuantityUnit",
                table: "WasteReports",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Treatment",
                table: "WasteReports",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractorName",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "DisposalCost",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "DisposalDate",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "DisposalMethod",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "DisposedBy",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "EstimatedQuantity",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "ManifestNumber",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "QuantityUnit",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "Treatment",
                table: "WasteReports");
        }
    }
}
