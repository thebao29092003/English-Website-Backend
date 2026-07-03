using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace English.Website.Migrations
{
    /// <inheritdoc />
    public partial class SaveTypeInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TypeAnalyse",
                table: "AISpeechToText",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TypeAnalyse",
                table: "AISpeechToText",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
