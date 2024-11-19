using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11192024_Migration_04PM40 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShareQuantity",
                table: "ShareEquity");

            migrationBuilder.DropColumn(
                name: "RemainingShares",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "TotalShares",
                table: "Project");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShareQuantity",
                table: "ShareEquity",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RemainingShares",
                table: "Project",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalShares",
                table: "Project",
                type: "integer",
                nullable: true);
        }
    }
}
