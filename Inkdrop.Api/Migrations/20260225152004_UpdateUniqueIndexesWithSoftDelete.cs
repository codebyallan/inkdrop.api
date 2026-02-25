using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inkdrop.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUniqueIndexesWithSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_Toners_Model_Manufacturer_Color",
                table: "Toners",
                columns: new[] { "Model", "Manufacturer", "Color" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Printers_IpAddress",
                table: "Printers",
                column: "IpAddress",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                table: "Locations",
                column: "Name",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
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
    }
}
