using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeestjeOpJeFeestje_PROG6.data.Migrations
{
    /// <inheritdoc />
    public partial class Maxweethetniet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "price",
                table: "Bookings",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "discount",
                table: "Bookings",
                newName: "Discount");

            migrationBuilder.AddColumn<int>(
                name: "PhoneNumber",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Bookings",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "Discount",
                table: "Bookings",
                newName: "discount");
        }
    }
}
