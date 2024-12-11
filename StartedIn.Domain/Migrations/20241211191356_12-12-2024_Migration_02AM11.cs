using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12122024_Migration_02AM11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAgreed",
                table: "TerminationRequest");

            migrationBuilder.RenameColumn(
                name: "ToId",
                table: "TerminationRequest",
                newName: "Status");

            migrationBuilder.CreateTable(
                name: "TerminationConfirmation",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TerminationRequestId = table.Column<string>(type: "text", nullable: false),
                    ConfirmUserId = table.Column<string>(type: "text", nullable: false),
                    IsAgreed = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminationConfirmation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TerminationConfirmation_TerminationRequest_TerminationReque~",
                        column: x => x.TerminationRequestId,
                        principalTable: "TerminationRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TerminationConfirmation_TerminationRequestId",
                table: "TerminationConfirmation",
                column: "TerminationRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TerminationConfirmation");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "TerminationRequest",
                newName: "ToId");

            migrationBuilder.AddColumn<bool>(
                name: "IsAgreed",
                table: "TerminationRequest",
                type: "boolean",
                nullable: true);
        }
    }
}
