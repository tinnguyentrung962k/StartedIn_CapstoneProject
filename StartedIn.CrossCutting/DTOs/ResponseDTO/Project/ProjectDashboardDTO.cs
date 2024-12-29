using StartedIn.CrossCutting.DTOs.ResponseDTO.Milestone;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Project
{
    public class ProjectDashboardDTO
    {
        public string? CurrentBudget { get; set; }
        public string? InAmount { get; set; }
        public string? OutAmount { get; set; }
        public string? TotalProfit { get; set; }
        public string? MonthProfit { get; set; }
        public string? RemainingDisbursement { get; set; }
        public string? DisbursedAmount { get; set; }
        public string? ShareEquityPercentage { get; set; }
        public List<MilestoneProgressResponseDTO>? MilestoneProgress { get; set; }
        public string? SelfRemainingDisbursement { get; set; }
        public string? SelfDisbursedAmount { get; set; }
        public List<TaskResponseDTO>? CompletedTasks { get; set; }
        public List<TaskResponseDTO>? OverdueTasks { get; set; }
        public int TotalTask { get; set; }
    }
}
