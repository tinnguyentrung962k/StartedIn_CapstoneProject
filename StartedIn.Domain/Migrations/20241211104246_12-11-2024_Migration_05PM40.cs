using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12112024_Migration_05PM40 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAgreedTermination",
                table: "UserContracts");

            migrationBuilder.DropColumn(
                name: "TerminationDate",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "TerminationInitiatorId",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "TerminationReason",
                table: "Contract");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAgreedTermination",
                table: "UserContracts",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "TerminationDate",
                table: "Contract",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TerminationInitiatorId",
                table: "Contract",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TerminationReason",
                table: "Contract",
                type: "text",
                nullable: true);
        }
    }
}
