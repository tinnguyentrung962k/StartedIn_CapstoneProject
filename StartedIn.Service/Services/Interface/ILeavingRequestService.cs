using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.DTOs.RequestDTO.LeavingRequest;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface ILeavingRequestService
    {
        Task<LeavingRequest> GetLeavingRequestById(string userId, string projectId, string requestId);
        Task<LeavingRequest> CreateLeavingRequest(string userId, string projectId, LeavingRequestCreateDTO leavingRequestCreateDTO);
        Task AcceptLeavingRequest(string userId, string projectId, string requestId, IFormFile confirmFile);
    }
}
