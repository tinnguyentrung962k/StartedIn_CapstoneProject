using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _051120241633 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contract_ContractIdNumber",
                table: "Contract");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Contract_ContractIdNumber",
                table: "Contract",
                column: "ContractIdNumber",
                unique: true);
        }
    }
}
