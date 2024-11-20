using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11202024_Migration_03AM30 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Asset_TransactionId",
                table: "Asset");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_TransactionId",
                table: "Asset",
                column: "TransactionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Asset_TransactionId",
                table: "Asset");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_TransactionId",
                table: "Asset",
                column: "TransactionId");
        }
    }
}
