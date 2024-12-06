using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvcCoreUploadAndDisplayImage_Demo.Data.Migrations
{
    /// <inheritdoc />
    public partial class RecuuringPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Histories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "Histories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "Histories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "Histories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "Histories");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Histories");
        }
    }
}
