using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rent_a_car.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverBioAndImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Biography",
                table: "Drivers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Drivers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Biography",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Drivers");
        }
    }
}
