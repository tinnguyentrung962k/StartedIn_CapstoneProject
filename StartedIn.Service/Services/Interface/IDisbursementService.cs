using StartedIn.CrossCutting.DTOs.RequestDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartedIn.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace StartedIn.Service.Services.Interface
{
    public interface IDisbursementService
    {
        Task FinishedTheTransaction(string disbursementId, string apiKey);
        Task CancelPayment(string disbursementId, string apiKey);
        Task ReminderDisbursementForInvestor();
        Task<PaginationDTO<DisbursementForLeaderInProjectResponseDTO>> GetDisbursementListForLeaderInAProject(string userId, string projectId, DisbursementFilterInProjectDTO disbursementFilterDTO, int size, int page);
        Task<PaginationDTO<DisbursementForInvestorInInvestorMenuResponseDTO>> GetDisbursementListForInvestorInMenu(string userId, DisbursementFilterInvestorMenuDTO disbursementFilterDTO, int size, int page);
        Task RejectADisbursement(string userId, string disbursementId, DisbursementRejectDTO disbursementRejectDTO);
        Task UpdateFinanceAndTransactionOfProjectOfFinishedDisbursement(User user, string projectId, Disbursement disbursement);
        Task DisbursementConfirmation(string userId, string projectId, string disbursementId);
        Task AutoUpdateOverdueIfDisbursementsExpire();
        Task AcceptDisbursement(string userId, string disbursementId, List<IFormFile> files);
        Task<Disbursement> GetADisbursementDetailForLeader(string userId, string projectId, string disbursementId);
        Task<Disbursement> GetADisbursementDetailInvestor(string userId, string disbursementId);
        Task<Disbursement> GetADisbursementDetailInProject(string userId, string projectId, string disbursementId);
        Task<SelfDisbursmentOfInvestorDTO> GetSelfDisbursementForInvestor(string userId, string projectId);
        Task<DisbursementOverviewOfProject> GetADisbursementTotalInAMonth(string projectId, DateTime dateTime);
        Task<PaginationDTO<DisbursementOverviewOfProjectForInvestor>> GetADisbursementOverviewForInvestor(string userId, int page, int size);
    }
}
