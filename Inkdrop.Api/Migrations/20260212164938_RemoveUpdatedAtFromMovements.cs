using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inkdrop.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUpdatedAtFromMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Movements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Movements",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
