using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11262024_Migration_19PM40 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "TaskComment",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TaskComment_UserId",
                table: "TaskComment",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskComment_User_UserId",
                table: "TaskComment",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskComment_User_UserId",
                table: "TaskComment");

            migrationBuilder.DropIndex(
                name: "IX_TaskComment_UserId",
                table: "TaskComment");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TaskComment");
        }
    }
}
