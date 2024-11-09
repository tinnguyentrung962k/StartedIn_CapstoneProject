using StartedIn.CrossCutting.DTOs.RequestDTO.Milestone;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IMilestoneService
    {
        Task<Milestone> CreateNewMilestone(string userId, string projectId, MilestoneCreateDTO milestoneCreateDto);
        Task<Milestone> GetMilestoneById(string userId, string projectId, string id);
        Task<Milestone> UpdateMilestoneInfo(string userId, string projectId, string id, MilestoneInfoUpdateDTO updateMilestoneInfoDTO);
        Task<List<Milestone>> GetMilestoneListOfAProject(string projectId);
    }
}
