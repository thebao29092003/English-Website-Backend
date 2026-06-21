using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace English.Website.Migrations
{
    /// <inheritdoc />
    public partial class AddRecoding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AudioDuration",
                table: "AISpeechToText");

            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "AISpeechToText");

            migrationBuilder.AddColumn<Guid>(
                name: "RecordingId",
                table: "AISpeechToText",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Recording",
                columns: table => new
                {
                    RecordingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CloudinaryPublicId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Duration = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recording", x => x.RecordingId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AISpeechToText_RecordingId",
                table: "AISpeechToText",
                column: "RecordingId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AISpeechToText_Recording_RecordingId",
                table: "AISpeechToText",
                column: "RecordingId",
                principalTable: "Recording",
                principalColumn: "RecordingId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AISpeechToText_Recording_RecordingId",
                table: "AISpeechToText");

            migrationBuilder.DropTable(
                name: "Recording");

            migrationBuilder.DropIndex(
                name: "IX_AISpeechToText_RecordingId",
                table: "AISpeechToText");

            migrationBuilder.DropColumn(
                name: "RecordingId",
                table: "AISpeechToText");

            migrationBuilder.AddColumn<double>(
                name: "AudioDuration",
                table: "AISpeechToText",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "AISpeechToText",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
