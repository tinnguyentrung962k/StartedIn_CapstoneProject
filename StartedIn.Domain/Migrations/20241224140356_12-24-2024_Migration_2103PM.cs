using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12242024_Migration_2103PM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppointmentUrl",
                table: "Project",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractId",
                table: "Appointment",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_ContractId",
                table: "Appointment",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Contract_ContractId",
                table: "Appointment",
                column: "ContractId",
                principalTable: "Contract",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Contract_ContractId",
                table: "Appointment");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_ContractId",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "AppointmentUrl",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "Appointment");
        }
    }
}
