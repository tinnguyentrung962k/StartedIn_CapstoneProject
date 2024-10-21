using StartedIn.CrossCutting.DTOs.RequestDTO;
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
        Task<Milestone> CreateNewMilestone(string userId, MilestoneCreateDTO milestoneCreateDto);
        Task<Milestone> GetMilestoneById(string id);
        Task<Milestone> UpdateMilestoneInfo(string id, MilestoneInfoUpdateDTO milestoneInfoUpdateDTO);
    }
}
