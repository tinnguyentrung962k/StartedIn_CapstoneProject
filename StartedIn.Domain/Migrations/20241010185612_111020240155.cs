using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _111020240155 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MilestoneHistory_MilestoneId",
                table: "MilestoneHistory",
                column: "MilestoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_MilestoneHistory_Milestone_MilestoneId",
                table: "MilestoneHistory",
                column: "MilestoneId",
                principalTable: "Milestone",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MilestoneHistory_Milestone_MilestoneId",
                table: "MilestoneHistory");

            migrationBuilder.DropIndex(
                name: "IX_MilestoneHistory_MilestoneId",
                table: "MilestoneHistory");
        }
    }
}
