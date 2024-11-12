using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11112024_Migration_10PM15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HarshChecksumPayOsKey",
                table: "Project",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HarshClientIdPayOsKey",
                table: "Project",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HarshPayOsApiKey",
                table: "Project",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HarshChecksumPayOsKey",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "HarshClientIdPayOsKey",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "HarshPayOsApiKey",
                table: "Project");
        }
    }
}
