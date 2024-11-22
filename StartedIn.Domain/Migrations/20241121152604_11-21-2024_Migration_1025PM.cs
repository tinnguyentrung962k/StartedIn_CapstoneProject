using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11212024_Migration_1025PM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Milestone_ProjectCharter_CharterId",
                table: "Milestone");

            migrationBuilder.RenameColumn(
                name: "PhaseName",
                table: "Milestone",
                newName: "PhaseId");

            migrationBuilder.RenameColumn(
                name: "CharterId",
                table: "Milestone",
                newName: "ProjectCharterId");

            migrationBuilder.RenameIndex(
                name: "IX_Milestone_CharterId",
                table: "Milestone",
                newName: "IX_Milestone_ProjectCharterId");

            migrationBuilder.CreateIndex(
                name: "IX_Milestone_PhaseId",
                table: "Milestone",
                column: "PhaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Milestone_Phases_PhaseId",
                table: "Milestone",
                column: "PhaseId",
                principalTable: "Phases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Milestone_ProjectCharter_ProjectCharterId",
                table: "Milestone",
                column: "ProjectCharterId",
                principalTable: "ProjectCharter",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Milestone_Phases_PhaseId",
                table: "Milestone");

            migrationBuilder.DropForeignKey(
                name: "FK_Milestone_ProjectCharter_ProjectCharterId",
                table: "Milestone");

            migrationBuilder.DropIndex(
                name: "IX_Milestone_PhaseId",
                table: "Milestone");

            migrationBuilder.RenameColumn(
                name: "ProjectCharterId",
                table: "Milestone",
                newName: "CharterId");

            migrationBuilder.RenameColumn(
                name: "PhaseId",
                table: "Milestone",
                newName: "PhaseName");

            migrationBuilder.RenameIndex(
                name: "IX_Milestone_ProjectCharterId",
                table: "Milestone",
                newName: "IX_Milestone_CharterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Milestone_ProjectCharter_CharterId",
                table: "Milestone",
                column: "CharterId",
                principalTable: "ProjectCharter",
                principalColumn: "Id");
        }
    }
}
