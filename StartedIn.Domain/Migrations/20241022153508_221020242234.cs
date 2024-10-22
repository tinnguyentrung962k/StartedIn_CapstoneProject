using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _221020242234 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MilestoneDate",
                table: "Milestone",
                newName: "DueDate");

            migrationBuilder.AlterColumn<decimal>(
                name: "Percentage",
                table: "Milestone",
                type: "numeric(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DueDate",
                table: "Milestone",
                newName: "MilestoneDate");

            migrationBuilder.AlterColumn<decimal>(
                name: "Percentage",
                table: "Milestone",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldNullable: true);
        }
    }
}
