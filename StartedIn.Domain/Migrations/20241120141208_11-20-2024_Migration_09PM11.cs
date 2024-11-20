using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11202024_Migration_09PM11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Asset_AssetId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_AssetId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "Transaction");

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "Asset",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_TransactionId",
                table: "Asset",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Transaction_TransactionId",
                table: "Asset",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asset_Transaction_TransactionId",
                table: "Asset");

            migrationBuilder.DropIndex(
                name: "IX_Asset_TransactionId",
                table: "Asset");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Asset");

            migrationBuilder.AddColumn<string>(
                name: "AssetId",
                table: "Transaction",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_AssetId",
                table: "Transaction",
                column: "AssetId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Asset_AssetId",
                table: "Transaction",
                column: "AssetId",
                principalTable: "Asset",
                principalColumn: "Id");
        }
    }
}
