using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12242024_Migration_03PM10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectDetail",
                table: "Project");

            migrationBuilder.AddColumn<string>(
                name: "ProjectDetailPost",
                table: "Project",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectDetailPost",
                table: "Project");

            migrationBuilder.AddColumn<string>(
                name: "ProjectDetail",
                table: "Project",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);
        }
    }
}
