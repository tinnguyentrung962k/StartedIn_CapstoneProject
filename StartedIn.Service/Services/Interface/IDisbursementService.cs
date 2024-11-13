using StartedIn.CrossCutting.DTOs.RequestDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IDisbursementService
    {
        Task FinishedTheTransaction(string disbursementId, string projectId, string apiKey);
        Task CancelPayment(string disbursementId, string projectId, string apiKey);
        Task ReminderDisbursementForInvestor();
        Task<PaginationDTO<DisbursementForLeaderInProjectResponseDTO>> GetDisbursementListForLeaderInAProject(string userId, string projectId, DisbursementFilterDTO disbursementFilterDTO, int size, int page);
    }
}
