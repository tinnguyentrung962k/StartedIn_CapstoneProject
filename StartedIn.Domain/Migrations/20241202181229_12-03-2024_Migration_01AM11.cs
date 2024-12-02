using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12032024_Migration_01AM11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectId",
                table: "Application",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Application_ProjectId",
                table: "Application",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Application_Project_ProjectId",
                table: "Application",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Application_Project_ProjectId",
                table: "Application");

            migrationBuilder.DropIndex(
                name: "IX_Application_ProjectId",
                table: "Application");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Application");
        }
    }
}
