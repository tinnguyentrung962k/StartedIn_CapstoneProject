using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11052024_Migration_10PM_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentTaskId",
                table: "Task",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Task_ParentTaskId",
                table: "Task",
                column: "ParentTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Task_ParentTaskId",
                table: "Task",
                column: "ParentTaskId",
                principalTable: "Task",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_Task_ParentTaskId",
                table: "Task");

            migrationBuilder.DropIndex(
                name: "IX_Task_ParentTaskId",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "ParentTaskId",
                table: "Task");
        }
    }
}
