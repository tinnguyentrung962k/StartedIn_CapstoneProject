using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12312024_Migration_11AM03 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Disbursement_Contract_ContractId",
                table: "Disbursement");

            migrationBuilder.AlterColumn<string>(
                name: "ContractId",
                table: "Disbursement",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "DealOfferId",
                table: "Disbursement",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Disbursement_Contract_ContractId",
                table: "Disbursement",
                column: "ContractId",
                principalTable: "Contract",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Disbursement_Contract_ContractId",
                table: "Disbursement");

            migrationBuilder.DropColumn(
                name: "DealOfferId",
                table: "Disbursement");

            migrationBuilder.AlterColumn<string>(
                name: "ContractId",
                table: "Disbursement",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Disbursement_Contract_ContractId",
                table: "Disbursement",
                column: "ContractId",
                principalTable: "Contract",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
