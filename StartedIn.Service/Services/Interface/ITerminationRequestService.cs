using StartedIn.CrossCutting.DTOs.RequestDTO.TerminationRequest;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TerminationRequest;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface ITerminationRequestService
    {
        Task CreateTerminationRequest(string userId, string projectId, string contractId, TerminationRequestCreateDTO requestCreateDTO);
        Task<List<TerminationRequestResponseDTO>> GetTerminationRequestForUserInProject(string userId, string projectId);
        Task AcceptTerminationRequest(string userId, string projectId, string requestId);
        Task RejectTerminationRequest(string userId, string projectId, string requestId);
        Task<TerminationRequestDetailDTO> GetContractTerminationDetail(string userId, string projectId, string requestId);
    }
}
