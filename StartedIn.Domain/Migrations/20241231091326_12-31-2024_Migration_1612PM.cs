using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12312024_Migration_1612PM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAppointment_Appointment_AppointmentId",
                table: "UserAppointment");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAppointment_User_UserId",
                table: "UserAppointment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAppointment",
                table: "UserAppointment");

            migrationBuilder.RenameTable(
                name: "UserAppointment",
                newName: "UserAppointments");

            migrationBuilder.RenameIndex(
                name: "IX_UserAppointment_AppointmentId",
                table: "UserAppointments",
                newName: "IX_UserAppointments_AppointmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAppointments",
                table: "UserAppointments",
                columns: new[] { "UserId", "AppointmentId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserAppointments_Appointment_AppointmentId",
                table: "UserAppointments",
                column: "AppointmentId",
                principalTable: "Appointment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAppointments_User_UserId",
                table: "UserAppointments",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAppointments_Appointment_AppointmentId",
                table: "UserAppointments");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAppointments_User_UserId",
                table: "UserAppointments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserAppointments",
                table: "UserAppointments");

            migrationBuilder.RenameTable(
                name: "UserAppointments",
                newName: "UserAppointment");

            migrationBuilder.RenameIndex(
                name: "IX_UserAppointments_AppointmentId",
                table: "UserAppointment",
                newName: "IX_UserAppointment_AppointmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserAppointment",
                table: "UserAppointment",
                columns: new[] { "UserId", "AppointmentId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserAppointment_Appointment_AppointmentId",
                table: "UserAppointment",
                column: "AppointmentId",
                principalTable: "Appointment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAppointment_User_UserId",
                table: "UserAppointment",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
