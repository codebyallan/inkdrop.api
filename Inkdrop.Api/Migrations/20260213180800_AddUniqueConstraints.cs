using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inkdrop.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Toners_Model_Manufacturer_Color",
                table: "Toners",
                columns: new[] { "Model", "Manufacturer", "Color" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Printers_IpAddress",
                table: "Printers",
                column: "IpAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                table: "Locations",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Toners_Model_Manufacturer_Color",
                table: "Toners");

            migrationBuilder.DropIndex(
                name: "IX_Printers_IpAddress",
                table: "Printers");

            migrationBuilder.DropIndex(
                name: "IX_Locations_Name",
                table: "Locations");
        }
    }
}
