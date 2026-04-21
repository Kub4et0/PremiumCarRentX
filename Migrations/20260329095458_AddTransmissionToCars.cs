using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rent_a_car.Migrations
{
    /// <inheritdoc />
    public partial class AddTransmissionToCars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Transmission",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Transmission",
                table: "Cars");
        }
    }
}
