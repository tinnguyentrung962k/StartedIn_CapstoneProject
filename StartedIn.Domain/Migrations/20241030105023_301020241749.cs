using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _301020241749 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DisbursementTitle",
                table: "Disbursement",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "DisbursementStartDate",
                table: "Disbursement",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "DisbursementEndDate",
                table: "Disbursement",
                newName: "EndDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Disbursement",
                newName: "DisbursementTitle");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Disbursement",
                newName: "DisbursementStartDate");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Disbursement",
                newName: "DisbursementEndDate");
        }
    }
}
