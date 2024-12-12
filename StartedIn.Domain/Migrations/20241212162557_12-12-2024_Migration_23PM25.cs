using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12122024_Migration_23PM25 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ContractPolicy",
                table: "Contract",
                type: "character varying(4500)",
                maxLength: 4500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4500)",
                oldMaxLength: 4500);

            migrationBuilder.AddColumn<string>(
                name: "ParentContractId",
                table: "Contract",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contract_ParentContractId",
                table: "Contract",
                column: "ParentContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contract_Contract_ParentContractId",
                table: "Contract",
                column: "ParentContractId",
                principalTable: "Contract",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contract_Contract_ParentContractId",
                table: "Contract");

            migrationBuilder.DropIndex(
                name: "IX_Contract_ParentContractId",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "ParentContractId",
                table: "Contract");

            migrationBuilder.AlterColumn<string>(
                name: "ContractPolicy",
                table: "Contract",
                type: "character varying(4500)",
                maxLength: 4500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(4500)",
                oldMaxLength: 4500,
                oldNullable: true);
        }
    }
}
