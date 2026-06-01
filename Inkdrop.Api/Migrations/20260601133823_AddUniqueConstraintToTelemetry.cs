using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inkdrop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToTelemetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PrinterTelemetries_PrinterId_CollectedAt",
                table: "PrinterTelemetries");

            migrationBuilder.CreateIndex(
                name: "IX_PrinterTelemetries_PrinterId_CollectedAt",
                table: "PrinterTelemetries",
                columns: new[] { "PrinterId", "CollectedAt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PrinterTelemetries_PrinterId_CollectedAt",
                table: "PrinterTelemetries");

            migrationBuilder.CreateIndex(
                name: "IX_PrinterTelemetries_PrinterId_CollectedAt",
                table: "PrinterTelemetries",
                columns: new[] { "PrinterId", "CollectedAt" });
        }
    }
}
