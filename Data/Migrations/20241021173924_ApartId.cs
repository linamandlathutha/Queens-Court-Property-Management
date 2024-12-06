using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcCoreUploadAndDisplayImage_Demo.Data.Migrations
{
    /// <inheritdoc />
    public partial class ApartId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Tests",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "ApartmentId",
                table: "ReportApartments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ReportApartments_ApartmentId",
                table: "ReportApartments",
                column: "ApartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportApartments_Apartments_ApartmentId",
                table: "ReportApartments",
                column: "ApartmentId",
                principalTable: "Apartments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportApartments_Apartments_ApartmentId",
                table: "ReportApartments");

            migrationBuilder.DropIndex(
                name: "IX_ReportApartments_ApartmentId",
                table: "ReportApartments");

            migrationBuilder.DropColumn(
                name: "ApartmentId",
                table: "ReportApartments");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Tests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16);
        }
    }
}
