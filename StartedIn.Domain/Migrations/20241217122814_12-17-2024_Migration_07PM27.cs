using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _12172024_Migration_07PM27 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "TransferDate",
                table: "TransferLeaderRequest",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "NewLeaderId",
                table: "TransferLeaderRequest",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_TransferLeaderRequest_FormerLeaderId",
                table: "TransferLeaderRequest",
                column: "FormerLeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferLeaderRequest_NewLeaderId",
                table: "TransferLeaderRequest",
                column: "NewLeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransferLeaderRequest_User_FormerLeaderId",
                table: "TransferLeaderRequest",
                column: "FormerLeaderId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransferLeaderRequest_User_NewLeaderId",
                table: "TransferLeaderRequest",
                column: "NewLeaderId",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransferLeaderRequest_User_FormerLeaderId",
                table: "TransferLeaderRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferLeaderRequest_User_NewLeaderId",
                table: "TransferLeaderRequest");

            migrationBuilder.DropIndex(
                name: "IX_TransferLeaderRequest_FormerLeaderId",
                table: "TransferLeaderRequest");

            migrationBuilder.DropIndex(
                name: "IX_TransferLeaderRequest_NewLeaderId",
                table: "TransferLeaderRequest");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "TransferDate",
                table: "TransferLeaderRequest",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NewLeaderId",
                table: "TransferLeaderRequest",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
