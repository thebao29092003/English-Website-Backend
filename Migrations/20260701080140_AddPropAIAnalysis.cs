using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace English.Website.Migrations
{
    /// <inheritdoc />
    public partial class AddPropAIAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "OverallGrammarScore",
                table: "AIAnalysis",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OverallVocabScore",
                table: "AIAnalysis",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverallGrammarScore",
                table: "AIAnalysis");

            migrationBuilder.DropColumn(
                name: "OverallVocabScore",
                table: "AIAnalysis");
        }
    }
}
