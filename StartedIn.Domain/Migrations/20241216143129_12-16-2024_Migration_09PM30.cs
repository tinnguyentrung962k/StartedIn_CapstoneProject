using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12162024_Migration_09PM30 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalActualManHour",
                table: "Task");

            migrationBuilder.AddColumn<string>(
                name: "TransferToId",
                table: "UserContracts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppointmentId",
                table: "TerminationRequest",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TerminationRequest_AppointmentId",
                table: "TerminationRequest",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TerminationRequest_Appointment_AppointmentId",
                table: "TerminationRequest",
                column: "AppointmentId",
                principalTable: "Appointment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TerminationRequest_Appointment_AppointmentId",
                table: "TerminationRequest");

            migrationBuilder.DropIndex(
                name: "IX_TerminationRequest_AppointmentId",
                table: "TerminationRequest");

            migrationBuilder.DropColumn(
                name: "TransferToId",
                table: "UserContracts");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "TerminationRequest");

            migrationBuilder.AddColumn<float>(
                name: "TotalActualManHour",
                table: "Task",
                type: "real",
                nullable: true);
        }
    }
}
