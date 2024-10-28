using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _281020241444 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentLink",
                table: "Contracts");

            migrationBuilder.AddColumn<string>(
                name: "SignNowDocumentId",
                table: "Contracts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShareEquity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ContractId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ShareQuantity = table.Column<int>(type: "integer", nullable: true),
                    Percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    StakeHolderType = table.Column<string>(type: "text", nullable: true),
                    DateAssigned = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareEquity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShareEquity_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShareEquity_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShareEquity_ContractId",
                table: "ShareEquity",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ShareEquity_UserId",
                table: "ShareEquity",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShareEquity");

            migrationBuilder.DropColumn(
                name: "SignNowDocumentId",
                table: "Contracts");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentLink",
                table: "Contracts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
