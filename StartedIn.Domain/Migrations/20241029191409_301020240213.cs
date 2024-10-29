using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _301020240213 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DealOffer",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<string>(type: "text", nullable: false),
                    InvestorId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    EquityShareOffer = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TermCondition = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DealStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealOffer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DealOffer_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DealOffer_User_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DealOfferHistory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DealOfferId = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealOfferHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DealOfferHistory_DealOffer_DealOfferId",
                        column: x => x.DealOfferId,
                        principalTable: "DealOffer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DealOffer_InvestorId",
                table: "DealOffer",
                column: "InvestorId");

            migrationBuilder.CreateIndex(
                name: "IX_DealOffer_ProjectId",
                table: "DealOffer",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DealOfferHistory_DealOfferId",
                table: "DealOfferHistory",
                column: "DealOfferId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DealOfferHistory");

            migrationBuilder.DropTable(
                name: "DealOffer");
        }
    }
}
