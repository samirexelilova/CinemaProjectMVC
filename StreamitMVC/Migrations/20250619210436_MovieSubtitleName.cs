using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamitMVC.Migrations
{
    /// <inheritdoc />
    public partial class MovieSubtitleName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Subtitles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Subtitles");
        }
    }
}
