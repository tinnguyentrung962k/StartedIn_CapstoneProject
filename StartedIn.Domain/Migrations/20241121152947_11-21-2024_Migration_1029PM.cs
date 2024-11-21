using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11212024_Migration_1029PM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Milestone_ProjectCharter_ProjectCharterId",
                table: "Milestone");

            migrationBuilder.DropIndex(
                name: "IX_Milestone_ProjectCharterId",
                table: "Milestone");

            migrationBuilder.DropColumn(
                name: "ProjectCharterId",
                table: "Milestone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectCharterId",
                table: "Milestone",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Milestone_ProjectCharterId",
                table: "Milestone",
                column: "ProjectCharterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Milestone_ProjectCharter_ProjectCharterId",
                table: "Milestone",
                column: "ProjectCharterId",
                principalTable: "ProjectCharter",
                principalColumn: "Id");
        }
    }
}
