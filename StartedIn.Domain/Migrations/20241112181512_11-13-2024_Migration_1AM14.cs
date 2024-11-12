using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _11132024_Migration_1AM14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Appointment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<string>(type: "text", nullable: false),
                    MilestoneId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AppointmentTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    MeetingLink = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointment_Milestone_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointment_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Document",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<string>(type: "text", nullable: false),
                    DocumentName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AttachmentLink = table.Column<string>(type: "text", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Document_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Finance",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<string>(type: "text", nullable: false),
                    CurrentBudget = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    TotalExpense = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    RemainingDisbursement = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    DisbursedAmount = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Finance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Finance_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetingNote",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AppointmentId = table.Column<string>(type: "text", nullable: false),
                    HostId = table.Column<string>(type: "text", nullable: false),
                    SecretaryId = table.Column<string>(type: "text", nullable: false),
                    MeetingContent = table.Column<string>(type: "text", nullable: false),
                    Conclusion = table.Column<string>(type: "text", nullable: false),
                    SignNowFileId = table.Column<string>(type: "text", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingNote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingNote_Appointment_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentComment",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DocumentId = table.Column<string>(type: "text", nullable: false),
                    CommentUserId = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentComment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentComment_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentComment_User_CommentUserId",
                        column: x => x.CommentUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FinanceId = table.Column<string>(type: "text", nullable: false),
                    DisbursementId = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    Budget = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    FromID = table.Column<string>(type: "text", nullable: false),
                    ToID = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    EvidenceUrl = table.Column<string>(type: "text", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastUpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_Disbursement_DisbursementId",
                        column: x => x.DisbursementId,
                        principalTable: "Disbursement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transaction_Finance_FinanceId",
                        column: x => x.FinanceId,
                        principalTable: "Finance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_MilestoneId",
                table: "Appointment",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_ProjectId",
                table: "Appointment",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_ProjectId",
                table: "Document",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentComment_CommentUserId",
                table: "DocumentComment",
                column: "CommentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentComment_DocumentId",
                table: "DocumentComment",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Finance_ProjectId",
                table: "Finance",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeetingNote_AppointmentId",
                table: "MeetingNote",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_DisbursementId",
                table: "Transaction",
                column: "DisbursementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_FinanceId",
                table: "Transaction",
                column: "FinanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentComment");

            migrationBuilder.DropTable(
                name: "MeetingNote");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "Document");

            migrationBuilder.DropTable(
                name: "Appointment");

            migrationBuilder.DropTable(
                name: "Finance");
        }
    }
}
