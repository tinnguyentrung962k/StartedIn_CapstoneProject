using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11222024_Migration_11PM49 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Milestone_Phase_PhaseId",
                table: "Milestone");

            migrationBuilder.AlterColumn<string>(
                name: "PhaseId",
                table: "Milestone",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Milestone_Phase_PhaseId",
                table: "Milestone",
                column: "PhaseId",
                principalTable: "Phase",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Milestone_Phase_PhaseId",
                table: "Milestone");

            migrationBuilder.AlterColumn<string>(
                name: "PhaseId",
                table: "Milestone",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Milestone_Phase_PhaseId",
                table: "Milestone",
                column: "PhaseId",
                principalTable: "Phase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
