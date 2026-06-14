using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace English.Website.Migrations
{
    /// <inheritdoc />
    public partial class addCurrentLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConcurrencyLimit",
                table: "AIModelText",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrencyLimit",
                table: "AIModelText");
        }
    }
}
