using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11082024_Migration_6PM4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DealOfferId",
                table: "Contract",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contract_DealOfferId",
                table: "Contract",
                column: "DealOfferId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contract_DealOffer_DealOfferId",
                table: "Contract",
                column: "DealOfferId",
                principalTable: "DealOffer",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contract_DealOffer_DealOfferId",
                table: "Contract");

            migrationBuilder.DropIndex(
                name: "IX_Contract_DealOfferId",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "DealOfferId",
                table: "Contract");
        }
    }
}
