using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zilor.AICopilot.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class AddUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_language_models_provider_name",
                table: "language_models",
                columns: new[] { "provider", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_conversation_templates_name",
                table: "conversation_templates",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_language_models_provider_name",
                table: "language_models");

            migrationBuilder.DropIndex(
                name: "IX_conversation_templates_name",
                table: "conversation_templates");
        }
    }
}
