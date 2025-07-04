using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamitMVC.Migrations
{
    /// <inheritdoc />
    public partial class MoviePurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SeatId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MoviePurchaseId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DirectPurchasePrice",
                table: "Movies",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableForDirectPurchase",
                table: "Movies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MovieId",
                table: "BasketItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "BasketItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MoviePurchases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MovieId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoviePurchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoviePurchases_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MoviePurchases_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MoviePurchaseId",
                table: "Payments",
                column: "MoviePurchaseId",
                unique: true,
                filter: "[MoviePurchaseId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BasketItems_MovieId",
                table: "BasketItems",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_MoviePurchases_MovieId",
                table: "MoviePurchases",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_MoviePurchases_UserId",
                table: "MoviePurchases",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BasketItems_Movies_MovieId",
                table: "BasketItems",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_MoviePurchases_MoviePurchaseId",
                table: "Payments",
                column: "MoviePurchaseId",
                principalTable: "MoviePurchases",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BasketItems_Movies_MovieId",
                table: "BasketItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_MoviePurchases_MoviePurchaseId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "MoviePurchases");

            migrationBuilder.DropIndex(
                name: "IX_Payments_MoviePurchaseId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_BasketItems_MovieId",
                table: "BasketItems");

            migrationBuilder.DropColumn(
                name: "MoviePurchaseId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DirectPurchasePrice",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "IsAvailableForDirectPurchase",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "MovieId",
                table: "BasketItems");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "BasketItems");

            migrationBuilder.AlterColumn<int>(
                name: "SeatId",
                table: "Tickets",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
