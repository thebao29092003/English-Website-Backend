using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace English.Website.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeAnalyse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeAnalyse",
                table: "AISpeechToText",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeAnalyse",
                table: "AISpeechToText");
        }
    }
}
