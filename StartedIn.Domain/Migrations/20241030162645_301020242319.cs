using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _301020242319 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Project_ProjectId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Disbursement_Contracts_ContractId",
                table: "Disbursement");

            migrationBuilder.DropForeignKey(
                name: "FK_ShareEquity_Contracts_ContractId",
                table: "ShareEquity");

            migrationBuilder.DropForeignKey(
                name: "FK_UserContracts_Contracts_ContractId",
                table: "UserContracts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contracts",
                table: "Contracts");

            migrationBuilder.RenameTable(
                name: "Contracts",
                newName: "Contract");

            migrationBuilder.RenameIndex(
                name: "IX_Contracts_ProjectId",
                table: "Contract",
                newName: "IX_Contract_ProjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contract",
                table: "Contract",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contract_Project_ProjectId",
                table: "Contract",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Disbursement_Contract_ContractId",
                table: "Disbursement",
                column: "ContractId",
                principalTable: "Contract",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShareEquity_Contract_ContractId",
                table: "ShareEquity",
                column: "ContractId",
                principalTable: "Contract",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserContracts_Contract_ContractId",
                table: "UserContracts",
                column: "ContractId",
                principalTable: "Contract",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contract_Project_ProjectId",
                table: "Contract");

            migrationBuilder.DropForeignKey(
                name: "FK_Disbursement_Contract_ContractId",
                table: "Disbursement");

            migrationBuilder.DropForeignKey(
                name: "FK_ShareEquity_Contract_ContractId",
                table: "ShareEquity");

            migrationBuilder.DropForeignKey(
                name: "FK_UserContracts_Contract_ContractId",
                table: "UserContracts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Contract",
                table: "Contract");

            migrationBuilder.RenameTable(
                name: "Contract",
                newName: "Contracts");

            migrationBuilder.RenameIndex(
                name: "IX_Contract_ProjectId",
                table: "Contracts",
                newName: "IX_Contracts_ProjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Contracts",
                table: "Contracts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Project_ProjectId",
                table: "Contracts",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Disbursement_Contracts_ContractId",
                table: "Disbursement",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShareEquity_Contracts_ContractId",
                table: "ShareEquity",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserContracts_Contracts_ContractId",
                table: "UserContracts",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
