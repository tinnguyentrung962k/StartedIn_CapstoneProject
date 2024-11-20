using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11202024_Migration_17AM26 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FromName",
                table: "Transaction",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToName",
                table: "Transaction",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromName",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "ToName",
                table: "Transaction");
        }
    }
}
