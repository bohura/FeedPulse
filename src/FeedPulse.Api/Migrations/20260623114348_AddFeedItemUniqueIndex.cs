using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedPulse.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedItemUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeedItems_FeedId",
                table: "FeedItems");

            migrationBuilder.CreateIndex(
                name: "IX_FeedItems_FeedId_ExternalId",
                table: "FeedItems",
                columns: new[] { "FeedId", "ExternalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FeedItems_FeedId_ExternalId",
                table: "FeedItems");

            migrationBuilder.CreateIndex(
                name: "IX_FeedItems_FeedId",
                table: "FeedItems",
                column: "FeedId");
        }
    }
}
