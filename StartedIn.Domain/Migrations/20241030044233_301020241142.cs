using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _301020241142 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Disbursement",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ContractId = table.Column<string>(type: "text", nullable: false),
                    InvestorId = table.Column<string>(type: "text", nullable: false),
                    DisbursementTitle = table.Column<string>(type: "text", nullable: false),
                    DisbursementStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DisbursementEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    Condition = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisbursementStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeclineReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExecutedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OrderCode = table.Column<long>(type: "bigint", nullable: false),
                    DisbursementMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disbursement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Disbursement_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Disbursement_User_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DisbursementAttachment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DisbursementId = table.Column<string>(type: "text", nullable: false),
                    EvidenceFile = table.Column<string>(type: "text", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisbursementAttachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisbursementAttachment_Disbursement_DisbursementId",
                        column: x => x.DisbursementId,
                        principalTable: "Disbursement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Disbursement_ContractId",
                table: "Disbursement",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Disbursement_InvestorId",
                table: "Disbursement",
                column: "InvestorId");

            migrationBuilder.CreateIndex(
                name: "IX_DisbursementAttachment_DisbursementId",
                table: "DisbursementAttachment",
                column: "DisbursementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisbursementAttachment");

            migrationBuilder.DropTable(
                name: "Disbursement");
        }
    }
}
