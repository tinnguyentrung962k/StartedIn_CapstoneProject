using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12032024_Migration_05PM00 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Milestone_MilestoneId",
                table: "Appointment");

            migrationBuilder.AlterColumn<string>(
                name: "MilestoneId",
                table: "Appointment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Milestone_MilestoneId",
                table: "Appointment",
                column: "MilestoneId",
                principalTable: "Milestone",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Milestone_MilestoneId",
                table: "Appointment");

            migrationBuilder.AlterColumn<string>(
                name: "MilestoneId",
                table: "Appointment",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Milestone_MilestoneId",
                table: "Appointment",
                column: "MilestoneId",
                principalTable: "Milestone",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
