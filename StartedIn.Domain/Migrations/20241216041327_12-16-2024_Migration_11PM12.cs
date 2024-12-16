using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12162024_Migration_11PM12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "ActualManHour",
                table: "UserTasks",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FinishTime",
                table: "UserTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartTime",
                table: "UserTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "TotalActualManHour",
                table: "Task",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualManHour",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "FinishTime",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "TotalActualManHour",
                table: "Task");
        }
    }
}
