using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _181020242110 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "Milestone");

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
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "Milestone",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
