using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11202024_Migration_4PM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtendedCount",
                table: "Milestone");

            migrationBuilder.DropColumn(
                name: "ExtendedDate",
                table: "Milestone");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "Milestone");

            migrationBuilder.RenameColumn(
                name: "DueDate",
                table: "Milestone",
                newName: "StartDate");

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "Milestone",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Milestone");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Milestone",
                newName: "DueDate");

            migrationBuilder.AddColumn<int>(
                name: "ExtendedCount",
                table: "Milestone",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExtendedDate",
                table: "Milestone",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Percentage",
                table: "Milestone",
                type: "numeric(5,2)",
                nullable: true);
        }
    }
}
