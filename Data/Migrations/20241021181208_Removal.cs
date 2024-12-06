using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcCoreUploadAndDisplayImage_Demo.Data.Migrations
{
    /// <inheritdoc />
    public partial class Removal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "ReportApartments");

            migrationBuilder.DropColumn(
                name: "ReportId",
                table: "ReportApartments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "ReportApartments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ReportId",
                table: "ReportApartments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
