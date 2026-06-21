using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace English.Website.Migrations
{
    /// <inheritdoc />
    public partial class AddAudioToSpeech : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenUsage_AIModelText_AIModelTextId",
                table: "TokenUsage");

            migrationBuilder.DropTable(
                name: "AiAnalyse");

            migrationBuilder.AddColumn<Guid>(
                name: "AIAnalysisId",
                table: "TokenUsage",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AIModelAudio",
                columns: table => new
                {
                    AIModelAudioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PricePerSecond = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIModelAudio", x => x.AIModelAudioId);
                });

            migrationBuilder.CreateTable(
                name: "AISpeechToText",
                columns: table => new
                {
                    AISpeechToTextId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssemblyAiId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AudioUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AITranscript = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AudioDuration = table.Column<double>(type: "float", nullable: false),
                    OverallConfidence = table.Column<double>(type: "float", nullable: false),
                    WordsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FluencyScore = table.Column<double>(type: "float", nullable: false),
                    PronunciationScore = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AISpeechToText", x => x.AISpeechToTextId);
                });

            migrationBuilder.CreateTable(
                name: "AIAnalysis",
                columns: table => new
                {
                    AIAnalysisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AISpeechToTextId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserTranscript = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnalysisContentJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIAnalysis", x => x.AIAnalysisId);
                    table.ForeignKey(
                        name: "FK_AIAnalysis_AISpeechToText_AISpeechToTextId",
                        column: x => x.AISpeechToTextId,
                        principalTable: "AISpeechToText",
                        principalColumn: "AISpeechToTextId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AudioUsage",
                columns: table => new
                {
                    AudioUsageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AIModelAudioId = table.Column<int>(type: "int", nullable: false),
                    AISpeechToTextId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalculatedCost = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioUsage", x => x.AudioUsageId);
                    table.ForeignKey(
                        name: "FK_AudioUsage_AIModelAudio_AIModelAudioId",
                        column: x => x.AIModelAudioId,
                        principalTable: "AIModelAudio",
                        principalColumn: "AIModelAudioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AudioUsage_AISpeechToText_AISpeechToTextId",
                        column: x => x.AISpeechToTextId,
                        principalTable: "AISpeechToText",
                        principalColumn: "AISpeechToTextId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokenUsage_AIAnalysisId",
                table: "TokenUsage",
                column: "AIAnalysisId",
                unique: true,
                filter: "[AIAnalysisId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AIAnalysis_AISpeechToTextId",
                table: "AIAnalysis",
                column: "AISpeechToTextId",
                unique: true,
                filter: "[AISpeechToTextId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AudioUsage_AIModelAudioId",
                table: "AudioUsage",
                column: "AIModelAudioId");

            migrationBuilder.CreateIndex(
                name: "IX_AudioUsage_AISpeechToTextId",
                table: "AudioUsage",
                column: "AISpeechToTextId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TokenUsage_AIAnalysis_AIAnalysisId",
                table: "TokenUsage",
                column: "AIAnalysisId",
                principalTable: "AIAnalysis",
                principalColumn: "AIAnalysisId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TokenUsage_AIModelText_AIModelTextId",
                table: "TokenUsage",
                column: "AIModelTextId",
                principalTable: "AIModelText",
                principalColumn: "AIModelTextId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TokenUsage_AIAnalysis_AIAnalysisId",
                table: "TokenUsage");

            migrationBuilder.DropForeignKey(
                name: "FK_TokenUsage_AIModelText_AIModelTextId",
                table: "TokenUsage");

            migrationBuilder.DropTable(
                name: "AIAnalysis");

            migrationBuilder.DropTable(
                name: "AudioUsage");

            migrationBuilder.DropTable(
                name: "AIModelAudio");

            migrationBuilder.DropTable(
                name: "AISpeechToText");

            migrationBuilder.DropIndex(
                name: "IX_TokenUsage_AIAnalysisId",
                table: "TokenUsage");

            migrationBuilder.DropColumn(
                name: "AIAnalysisId",
                table: "TokenUsage");

            migrationBuilder.CreateTable(
                name: "AiAnalyse",
                columns: table => new
                {
                    AiAnalysisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenUsageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnalysisContentJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserTranscript = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAnalyse", x => x.AiAnalysisId);
                    table.ForeignKey(
                        name: "FK_AiAnalyse_TokenUsage_TokenUsageId",
                        column: x => x.TokenUsageId,
                        principalTable: "TokenUsage",
                        principalColumn: "TokenUsageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiAnalyse_TokenUsageId",
                table: "AiAnalyse",
                column: "TokenUsageId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TokenUsage_AIModelText_AIModelTextId",
                table: "TokenUsage",
                column: "AIModelTextId",
                principalTable: "AIModelText",
                principalColumn: "AIModelTextId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
