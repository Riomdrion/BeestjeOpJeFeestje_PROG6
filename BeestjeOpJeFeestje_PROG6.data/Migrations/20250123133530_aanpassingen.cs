using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeestjeOpJeFeestje_PROG6.data.Migrations
{
    /// <inheritdoc />
    public partial class aanpassingen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Animals_AnimalId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_AnimalId",
                table: "Bookings");

            migrationBuilder.AddColumn<string>(
                name: "adress",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "discount",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "BookingAnimal",
                columns: table => new
                {
                    AnimalId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingAnimal", x => new { x.AnimalId, x.BookingId });
                    table.ForeignKey(
                        name: "FK_BookingAnimal_Animals_AnimalId",
                        column: x => x.AnimalId,
                        principalTable: "Animals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingAnimal_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingAnimal_BookingId",
                table: "BookingAnimal",
                column: "BookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingAnimal");

            migrationBuilder.DropColumn(
                name: "adress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "discount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "price",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_AnimalId",
                table: "Bookings",
                column: "AnimalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Animals_AnimalId",
                table: "Bookings",
                column: "AnimalId",
                principalTable: "Animals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
