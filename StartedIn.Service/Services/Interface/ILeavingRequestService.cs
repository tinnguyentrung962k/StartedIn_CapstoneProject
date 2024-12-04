using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.DTOs.RequestDTO.LeavingRequest;
using StartedIn.CrossCutting.DTOs.ResponseDTO.LeavingRequest;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
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
        Task RejectLeavingRequest(string userId, string projectId, string requestId);
        Task<PaginationDTO<LeavingRequestResponseDTO>> FilterLeavingRequestForLeader(string userId, string projectId, LeavingRequestFilterDTO leavingRequestFilterDTO, int page, int size);
    }
}
