using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11142024_Migration_11AM40 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectCharter_ProjectId",
                table: "ProjectCharter");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "DisbursementAttachment",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCharter_ProjectId",
                table: "ProjectCharter",
                column: "ProjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectCharter_ProjectId",
                table: "ProjectCharter");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "DisbursementAttachment");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectCharter_ProjectId",
                table: "ProjectCharter",
                column: "ProjectId");
        }
    }
}
