using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _081020241535 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistories_Task_TaskId",
                table: "TaskHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskHistories",
                table: "TaskHistories");

            migrationBuilder.RenameTable(
                name: "TaskHistories",
                newName: "TaskHistory");

            migrationBuilder.RenameIndex(
                name: "IX_TaskHistories_TaskId",
                table: "TaskHistory",
                newName: "IX_TaskHistory_TaskId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskHistory",
                table: "TaskHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistory_Task_TaskId",
                table: "TaskHistory",
                column: "TaskId",
                principalTable: "Task",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistory_Task_TaskId",
                table: "TaskHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskHistory",
                table: "TaskHistory");

            migrationBuilder.RenameTable(
                name: "TaskHistory",
                newName: "TaskHistories");

            migrationBuilder.RenameIndex(
                name: "IX_TaskHistory_TaskId",
                table: "TaskHistories",
                newName: "IX_TaskHistories_TaskId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskHistories",
                table: "TaskHistories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistories_Task_TaskId",
                table: "TaskHistories",
                column: "TaskId",
                principalTable: "Task",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
