using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12132024_Migration_05PM31 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Conclusion",
                table: "MeetingNote");

            migrationBuilder.DropColumn(
                name: "HostId",
                table: "MeetingNote");

            migrationBuilder.DropColumn(
                name: "MeetingContent",
                table: "MeetingNote");

            migrationBuilder.DropColumn(
                name: "SecretaryId",
                table: "MeetingNote");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Conclusion",
                table: "MeetingNote",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HostId",
                table: "MeetingNote",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MeetingContent",
                table: "MeetingNote",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecretaryId",
                table: "MeetingNote",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
