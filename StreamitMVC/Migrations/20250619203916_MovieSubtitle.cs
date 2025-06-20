using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamitMVC.Migrations
{
    /// <inheritdoc />
    public partial class MovieSubtitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubtitleId",
                table: "Sessions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SubtitleId",
                table: "Sessions",
                column: "SubtitleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Subtitles_SubtitleId",
                table: "Sessions",
                column: "SubtitleId",
                principalTable: "Subtitles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Subtitles_SubtitleId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_SubtitleId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "SubtitleId",
                table: "Sessions");
        }
    }
}
