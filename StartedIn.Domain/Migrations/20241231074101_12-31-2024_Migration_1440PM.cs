using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12312024_Migration_1440PM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_ProjectApprovals_ProjectApprovalId",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "DisbursementMethod",
                table: "Disbursement");

            migrationBuilder.DropColumn(
                name: "ExecutedTime",
                table: "Disbursement");

            migrationBuilder.AlterColumn<string>(
                name: "ProjectApprovalId",
                table: "Document",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "AppointmentId",
                table: "Document",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserAppointment",
                columns: table => new
                {
                    AppointmentId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAppointment", x => new { x.UserId, x.AppointmentId });
                    table.ForeignKey(
                        name: "FK_UserAppointment_Appointment_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAppointment_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Document_AppointmentId",
                table: "Document",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAppointment_AppointmentId",
                table: "UserAppointment",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Appointment_AppointmentId",
                table: "Document",
                column: "AppointmentId",
                principalTable: "Appointment",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_ProjectApprovals_ProjectApprovalId",
                table: "Document",
                column: "ProjectApprovalId",
                principalTable: "ProjectApprovals",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_Appointment_AppointmentId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_ProjectApprovals_ProjectApprovalId",
                table: "Document");

            migrationBuilder.DropTable(
                name: "UserAppointment");

            migrationBuilder.DropIndex(
                name: "IX_Document_AppointmentId",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "Document");

            migrationBuilder.AlterColumn<string>(
                name: "ProjectApprovalId",
                table: "Document",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisbursementMethod",
                table: "Disbursement",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExecutedTime",
                table: "Disbursement",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Document_ProjectApprovals_ProjectApprovalId",
                table: "Document",
                column: "ProjectApprovalId",
                principalTable: "ProjectApprovals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
