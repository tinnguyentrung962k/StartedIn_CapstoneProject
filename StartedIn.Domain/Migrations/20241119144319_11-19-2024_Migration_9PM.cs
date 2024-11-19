using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11192024_Migration_9PM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectCharter_ProjectId",
                table: "ProjectCharter");

            migrationBuilder.AddColumn<decimal>(
                name: "EquityShare",
                table: "InvestmentCall",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCharter_ProjectId",
                table: "ProjectCharter",
                column: "ProjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectCharter_ProjectId",
                table: "ProjectCharter");

            migrationBuilder.DropColumn(
                name: "EquityShare",
                table: "InvestmentCall");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCharter_ProjectId",
                table: "ProjectCharter",
                column: "ProjectId");
        }
    }
}
