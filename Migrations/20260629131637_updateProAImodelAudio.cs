using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace English.Website.Migrations
{
    /// <inheritdoc />
    public partial class updateProAImodelAudio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePerSecond",
                table: "AIModelAudio");

            migrationBuilder.AddColumn<double>(
                name: "PricePerHour",
                table: "AIModelAudio",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePerHour",
                table: "AIModelAudio");

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerSecond",
                table: "AIModelAudio",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
