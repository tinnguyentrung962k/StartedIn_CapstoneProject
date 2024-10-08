using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StartedIn.Domain.Migrations
{
    /// <inheritdoc />
    public partial class _081020241530 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Milestones_Phases_PhaseId",
                table: "Milestones");

            migrationBuilder.DropForeignKey(
                name: "FK_Phases_Projects_ProjectId",
                table: "Phases");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskAttachments_TaskEntities_TaskId",
                table: "TaskAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Taskboards_Milestones_MilestoneId",
                table: "Taskboards");

            migrationBuilder.DropForeignKey(
                name: "FK_Taskboards_Phases_PhaseId",
                table: "Taskboards");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskComments_TaskEntities_TaskId",
                table: "TaskComments");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskEntities_Milestones_MilestoneId",
                table: "TaskEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskEntities_Taskboards_TaskboardId",
                table: "TaskEntities");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistories_TaskEntities_TaskId",
                table: "TaskHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProject_Projects_ProjectId",
                table: "UserProject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskEntities",
                table: "TaskEntities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskComments",
                table: "TaskComments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Taskboards",
                table: "Taskboards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskAttachments",
                table: "TaskAttachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Projects",
                table: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Phases",
                table: "Phases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Milestones",
                table: "Milestones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MilestoneHistories",
                table: "MilestoneHistories");

            migrationBuilder.RenameTable(
                name: "TaskEntities",
                newName: "Task");

            migrationBuilder.RenameTable(
                name: "TaskComments",
                newName: "TaskComment");

            migrationBuilder.RenameTable(
                name: "Taskboards",
                newName: "Taskboard");

            migrationBuilder.RenameTable(
                name: "TaskAttachments",
                newName: "TaskAttachment");

            migrationBuilder.RenameTable(
                name: "Projects",
                newName: "Project");

            migrationBuilder.RenameTable(
                name: "Phases",
                newName: "Phase");

            migrationBuilder.RenameTable(
                name: "Milestones",
                newName: "Milestone");

            migrationBuilder.RenameTable(
                name: "MilestoneHistories",
                newName: "MilestoneHistory");

            migrationBuilder.RenameIndex(
                name: "IX_TaskEntities_TaskboardId",
                table: "Task",
                newName: "IX_Task_TaskboardId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskEntities_MilestoneId",
                table: "Task",
                newName: "IX_Task_MilestoneId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskComments_TaskId",
                table: "TaskComment",
                newName: "IX_TaskComment_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Taskboards_PhaseId",
                table: "Taskboard",
                newName: "IX_Taskboard_PhaseId");

            migrationBuilder.RenameIndex(
                name: "IX_Taskboards_MilestoneId",
                table: "Taskboard",
                newName: "IX_Taskboard_MilestoneId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskAttachments_TaskId",
                table: "TaskAttachment",
                newName: "IX_TaskAttachment_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Phases_ProjectId",
                table: "Phase",
                newName: "IX_Phase_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Milestones_PhaseId",
                table: "Milestone",
                newName: "IX_Milestone_PhaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Task",
                table: "Task",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskComment",
                table: "TaskComment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Taskboard",
                table: "Taskboard",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskAttachment",
                table: "TaskAttachment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Project",
                table: "Project",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Phase",
                table: "Phase",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Milestone",
                table: "Milestone",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MilestoneHistory",
                table: "MilestoneHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Milestone_Phase_PhaseId",
                table: "Milestone",
                column: "PhaseId",
                principalTable: "Phase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Phase_Project_ProjectId",
                table: "Phase",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Milestone_MilestoneId",
                table: "Task",
                column: "MilestoneId",
                principalTable: "Milestone",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Taskboard_TaskboardId",
                table: "Task",
                column: "TaskboardId",
                principalTable: "Taskboard",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAttachment_Task_TaskId",
                table: "TaskAttachment",
                column: "TaskId",
                principalTable: "Task",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Taskboard_Milestone_MilestoneId",
                table: "Taskboard",
                column: "MilestoneId",
                principalTable: "Milestone",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Taskboard_Phase_PhaseId",
                table: "Taskboard",
                column: "PhaseId",
                principalTable: "Phase",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskComment_Task_TaskId",
                table: "TaskComment",
                column: "TaskId",
                principalTable: "Task",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistories_Task_TaskId",
                table: "TaskHistories",
                column: "TaskId",
                principalTable: "Task",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProject_Project_ProjectId",
                table: "UserProject",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Milestone_Phase_PhaseId",
                table: "Milestone");

            migrationBuilder.DropForeignKey(
                name: "FK_Phase_Project_ProjectId",
                table: "Phase");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_Milestone_MilestoneId",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_Taskboard_TaskboardId",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskAttachment_Task_TaskId",
                table: "TaskAttachment");

            migrationBuilder.DropForeignKey(
                name: "FK_Taskboard_Milestone_MilestoneId",
                table: "Taskboard");

            migrationBuilder.DropForeignKey(
                name: "FK_Taskboard_Phase_PhaseId",
                table: "Taskboard");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskComment_Task_TaskId",
                table: "TaskComment");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskHistories_Task_TaskId",
                table: "TaskHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProject_Project_ProjectId",
                table: "UserProject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskComment",
                table: "TaskComment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Taskboard",
                table: "Taskboard");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskAttachment",
                table: "TaskAttachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Task",
                table: "Task");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Project",
                table: "Project");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Phase",
                table: "Phase");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MilestoneHistory",
                table: "MilestoneHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Milestone",
                table: "Milestone");

            migrationBuilder.RenameTable(
                name: "TaskComment",
                newName: "TaskComments");

            migrationBuilder.RenameTable(
                name: "Taskboard",
                newName: "Taskboards");

            migrationBuilder.RenameTable(
                name: "TaskAttachment",
                newName: "TaskAttachments");

            migrationBuilder.RenameTable(
                name: "Task",
                newName: "TaskEntities");

            migrationBuilder.RenameTable(
                name: "Project",
                newName: "Projects");

            migrationBuilder.RenameTable(
                name: "Phase",
                newName: "Phases");

            migrationBuilder.RenameTable(
                name: "MilestoneHistory",
                newName: "MilestoneHistories");

            migrationBuilder.RenameTable(
                name: "Milestone",
                newName: "Milestones");

            migrationBuilder.RenameIndex(
                name: "IX_TaskComment_TaskId",
                table: "TaskComments",
                newName: "IX_TaskComments_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Taskboard_PhaseId",
                table: "Taskboards",
                newName: "IX_Taskboards_PhaseId");

            migrationBuilder.RenameIndex(
                name: "IX_Taskboard_MilestoneId",
                table: "Taskboards",
                newName: "IX_Taskboards_MilestoneId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskAttachment_TaskId",
                table: "TaskAttachments",
                newName: "IX_TaskAttachments_TaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Task_TaskboardId",
                table: "TaskEntities",
                newName: "IX_TaskEntities_TaskboardId");

            migrationBuilder.RenameIndex(
                name: "IX_Task_MilestoneId",
                table: "TaskEntities",
                newName: "IX_TaskEntities_MilestoneId");

            migrationBuilder.RenameIndex(
                name: "IX_Phase_ProjectId",
                table: "Phases",
                newName: "IX_Phases_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Milestone_PhaseId",
                table: "Milestones",
                newName: "IX_Milestones_PhaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskComments",
                table: "TaskComments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Taskboards",
                table: "Taskboards",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskAttachments",
                table: "TaskAttachments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskEntities",
                table: "TaskEntities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Projects",
                table: "Projects",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Phases",
                table: "Phases",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MilestoneHistories",
                table: "MilestoneHistories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Milestones",
                table: "Milestones",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Milestones_Phases_PhaseId",
                table: "Milestones",
                column: "PhaseId",
                principalTable: "Phases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Phases_Projects_ProjectId",
                table: "Phases",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAttachments_TaskEntities_TaskId",
                table: "TaskAttachments",
                column: "TaskId",
                principalTable: "TaskEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Taskboards_Milestones_MilestoneId",
                table: "Taskboards",
                column: "MilestoneId",
                principalTable: "Milestones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Taskboards_Phases_PhaseId",
                table: "Taskboards",
                column: "PhaseId",
                principalTable: "Phases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskComments_TaskEntities_TaskId",
                table: "TaskComments",
                column: "TaskId",
                principalTable: "TaskEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskEntities_Milestones_MilestoneId",
                table: "TaskEntities",
                column: "MilestoneId",
                principalTable: "Milestones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskEntities_Taskboards_TaskboardId",
                table: "TaskEntities",
                column: "TaskboardId",
                principalTable: "Taskboards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskHistories_TaskEntities_TaskId",
                table: "TaskHistories",
                column: "TaskId",
                principalTable: "TaskEntities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProject_Projects_ProjectId",
                table: "UserProject",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
