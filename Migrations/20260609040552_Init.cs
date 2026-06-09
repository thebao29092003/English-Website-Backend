using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace English.Website.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIModelText",
                columns: table => new
                {
                    AIModelTextId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AIName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InputPricePerMillion = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    OutputPricePerMillion = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CacheHitPricePerMillion = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIModelText", x => x.AIModelTextId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "TokenUsage",
                columns: table => new
                {
                    TokenUsageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AIModelTextId = table.Column<int>(type: "int", nullable: false),
                    TotalTokens = table.Column<int>(type: "int", nullable: false),
                    PromptTokens = table.Column<int>(type: "int", nullable: false),
                    CacheHitTokens = table.Column<int>(type: "int", nullable: true),
                    CacheMissTokens = table.Column<int>(type: "int", nullable: true),
                    CompletionTokens = table.Column<int>(type: "int", nullable: false),
                    ReasoningTokens = table.Column<int>(type: "int", nullable: false),
                    CalculatedCost = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenUsage", x => x.TokenUsageId);
                    table.ForeignKey(
                        name: "FK_TokenUsage_AIModelText_AIModelTextId",
                        column: x => x.AIModelTextId,
                        principalTable: "AIModelText",
                        principalColumn: "AIModelTextId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AiAnalyse",
                columns: table => new
                {
                    AiAnalysisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenUsageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserTranscript = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnalysisContentJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_TokenUsage_AIModelTextId",
                table: "TokenUsage",
                column: "AIModelTextId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiAnalyse");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "TokenUsage");

            migrationBuilder.DropTable(
                name: "AIModelText");
        }
    }
}
