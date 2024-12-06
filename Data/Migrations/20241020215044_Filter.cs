using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcCoreUploadAndDisplayImage_Demo.Data.Migrations
{
    /// <inheritdoc />
    public partial class Filter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Reservations");
        }
    }
}
