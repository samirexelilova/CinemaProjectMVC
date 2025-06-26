using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamitMVC.Migrations
{
    /// <inheritdoc />
    public partial class FixedCascadeIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Subtitles_SubtitleId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subtitles_Movies_MovieId",
                table: "Subtitles");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Subtitles");

            migrationBuilder.CreateTable(
                name: "ReviewReactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsLike = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewReactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReviewReactions_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReactions_ReviewId",
                table: "ReviewReactions",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReactions_UserId",
                table: "ReviewReactions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Subtitles_SubtitleId",
                table: "Sessions",
                column: "SubtitleId",
                principalTable: "Subtitles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subtitles_Movies_MovieId",
                table: "Subtitles",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Subtitles_SubtitleId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subtitles_Movies_MovieId",
                table: "Subtitles");

            migrationBuilder.DropTable(
                name: "ReviewReactions");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Subtitles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Subtitles_SubtitleId",
                table: "Sessions",
                column: "SubtitleId",
                principalTable: "Subtitles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subtitles_Movies_MovieId",
                table: "Subtitles",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "Id");
        }
    }
}
