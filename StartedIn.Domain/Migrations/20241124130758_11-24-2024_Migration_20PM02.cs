using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11242024_Migration_20PM02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Application_Recruitment_RecruitmentId",
                table: "Application");

            migrationBuilder.AlterColumn<string>(
                name: "RecruitmentId",
                table: "Application",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CVUrl",
                table: "Application",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Application",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Application",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Application_Recruitment_RecruitmentId",
                table: "Application",
                column: "RecruitmentId",
                principalTable: "Recruitment",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Application_Recruitment_RecruitmentId",
                table: "Application");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Application");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Application");

            migrationBuilder.AlterColumn<string>(
                name: "RecruitmentId",
                table: "Application",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CVUrl",
                table: "Application",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Application_Recruitment_RecruitmentId",
                table: "Application",
                column: "RecruitmentId",
                principalTable: "Recruitment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
