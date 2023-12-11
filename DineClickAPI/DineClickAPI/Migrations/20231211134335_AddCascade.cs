using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DineClickAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_ReservingUserId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Restaurants_AspNetUsers_RestaurantManagerId",
                table: "Restaurants");

            migrationBuilder.AlterColumn<string>(
                name: "RestaurantManagerId",
                table: "Restaurants",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReservingUserId",
                table: "Reservations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_ReservingUserId",
                table: "Reservations",
                column: "ReservingUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Restaurants_AspNetUsers_RestaurantManagerId",
                table: "Restaurants",
                column: "RestaurantManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_ReservingUserId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Restaurants_AspNetUsers_RestaurantManagerId",
                table: "Restaurants");

            migrationBuilder.AlterColumn<string>(
                name: "RestaurantManagerId",
                table: "Restaurants",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ReservingUserId",
                table: "Reservations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_ReservingUserId",
                table: "Reservations",
                column: "ReservingUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Restaurants_AspNetUsers_RestaurantManagerId",
                table: "Restaurants",
                column: "RestaurantManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
