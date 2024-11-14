using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11142024_Migration_09PM00 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActiveCallId",
                table: "Project",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvestmentCallId",
                table: "DealOffer",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InvestmentCall",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<string>(type: "text", nullable: false),
                    TargetCall = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    AmountRaised = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TotalInvestor = table.Column<int>(type: "integer", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentCall", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestmentCall_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DealOffer_InvestmentCallId",
                table: "DealOffer",
                column: "InvestmentCallId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentCall_ProjectId",
                table: "InvestmentCall",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_DealOffer_InvestmentCall_InvestmentCallId",
                table: "DealOffer",
                column: "InvestmentCallId",
                principalTable: "InvestmentCall",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DealOffer_InvestmentCall_InvestmentCallId",
                table: "DealOffer");

            migrationBuilder.DropTable(
                name: "InvestmentCall");

            migrationBuilder.DropIndex(
                name: "IX_DealOffer_InvestmentCallId",
                table: "DealOffer");

            migrationBuilder.DropColumn(
                name: "ActiveCallId",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "InvestmentCallId",
                table: "DealOffer");
        }
    }
}
