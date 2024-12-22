using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TransferLeaderRequest;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface ITransferLeaderRequestService
    {
        Task CreateLeaderTransferRequestInAProject(string userId, string projectId, TerminationMeetingCreateDTO terminationMeetingCreateDTO);
        Task TransferLeaderAfterMeeting(string userId, string projectId, string requestId, LeaderTransferRequestDTO leaderTransferRequestDTO);
        Task<TransferLeaderRequest> GetPendingTransferLeaderRequest(string userId, string projectId);
        Task CancelTransferLeaderRequest(string userId, string projectId, string requestId);
        Task<PaginationDTO<TransferLeaderHistoryResponseDTO>> GetLeaderHistoryInTheProject(string userId, string projectId, int page, int size);
        Task<TransferLeaderHistoryResponseDTO> GetLeaderHistoryInTheProjectById(string userId, string projectId, string transferId);
    }
}
