using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12192024_Migration_0015AM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectApprovalId",
                table: "Document",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ProjectApprovals",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RejectReason = table.Column<string>(type: "text", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectApprovals_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Document_ProjectApprovalId",
                table: "Document",
                column: "ProjectApprovalId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectApprovals_ProjectId",
                table: "ProjectApprovals",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_ProjectApprovals_ProjectApprovalId",
                table: "Document",
                column: "ProjectApprovalId",
                principalTable: "ProjectApprovals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_ProjectApprovals_ProjectApprovalId",
                table: "Document");

            migrationBuilder.DropTable(
                name: "ProjectApprovals");

            migrationBuilder.DropIndex(
                name: "IX_Document_ProjectApprovalId",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "ProjectApprovalId",
                table: "Document");
        }
    }
}
