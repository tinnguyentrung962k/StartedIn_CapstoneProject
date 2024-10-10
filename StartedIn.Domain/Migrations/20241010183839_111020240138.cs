using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _111020240138 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Task_Milestone_MilestoneId",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_Taskboard_Phase_PhaseId",
                table: "Taskboard");

            migrationBuilder.DropIndex(
                name: "IX_Taskboard_PhaseId",
                table: "Taskboard");

            migrationBuilder.DropIndex(
                name: "IX_Task_MilestoneId",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "PhaseId",
                table: "Taskboard");

            migrationBuilder.DropColumn(
                name: "MilestoneId",
                table: "Task");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Deadline",
                table: "Task",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "Task",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "Milestone",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Task");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "Milestone");

            migrationBuilder.AddColumn<string>(
                name: "PhaseId",
                table: "Taskboard",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MilestoneId",
                table: "Task",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Taskboard_PhaseId",
                table: "Taskboard",
                column: "PhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Task_MilestoneId",
                table: "Task",
                column: "MilestoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Milestone_MilestoneId",
                table: "Task",
                column: "MilestoneId",
                principalTable: "Milestone",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Taskboard_Phase_PhaseId",
                table: "Taskboard",
                column: "PhaseId",
                principalTable: "Phase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
