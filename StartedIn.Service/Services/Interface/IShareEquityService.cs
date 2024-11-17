using StartedIn.CrossCutting.DTOs.RequestDTO.EquityShare;
using StartedIn.CrossCutting.DTOs.ResponseDTO.ShareEquity;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IShareEquityService
    {
        Task<List<ShareEquitySummaryDTO>> GetShareEquityOfAllMembersInAProject(string userId, string projectId, EquityShareFilterDTO equityShareFilterDTO);
    }
}
