using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11052024_Migration_9PM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLate",
                table: "Task",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLate",
                table: "Task");
        }
    }
}
