using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11252024_Migration_1143PM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Deadline",
                table: "Task",
                newName: "StartDate");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EndDate",
                table: "Task",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Task");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Task",
                newName: "Deadline");
        }
    }
}
