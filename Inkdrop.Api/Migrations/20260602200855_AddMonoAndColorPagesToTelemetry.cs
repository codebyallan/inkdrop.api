using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inkdrop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMonoAndColorPagesToTelemetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColorPages",
                table: "PrinterTelemetries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MonoPages",
                table: "PrinterTelemetries",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorPages",
                table: "PrinterTelemetries");

            migrationBuilder.DropColumn(
                name: "MonoPages",
                table: "PrinterTelemetries");
        }
    }
}
