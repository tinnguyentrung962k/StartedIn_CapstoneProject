using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12242024_Migration_12PM19 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TerminationRequest_Appointment_AppointmentId",
                table: "TerminationRequest");

            migrationBuilder.DropIndex(
                name: "IX_TerminationRequest_AppointmentId",
                table: "TerminationRequest");

            migrationBuilder.AddColumn<string>(
                name: "TerminationRequestId",
                table: "Appointment",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_TerminationRequestId",
                table: "Appointment",
                column: "TerminationRequestId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_TerminationRequest_TerminationRequestId",
                table: "Appointment",
                column: "TerminationRequestId",
                principalTable: "TerminationRequest",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_TerminationRequest_TerminationRequestId",
                table: "Appointment");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_TerminationRequestId",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "TerminationRequestId",
                table: "Appointment");

            migrationBuilder.CreateIndex(
                name: "IX_TerminationRequest_AppointmentId",
                table: "TerminationRequest",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TerminationRequest_Appointment_AppointmentId",
                table: "TerminationRequest",
                column: "AppointmentId",
                principalTable: "Appointment",
                principalColumn: "Id");
        }
    }
}
