using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IContractService
    {
        Task<Contract> CreateInvestmentContract(string userId, InvestmentContractCreateDTO investmentContractCreateDTO);
        Task<Contract> SendSigningInvitationForContract(string userId, string contractId);
        Task<IEnumerable<Contract>> GetContractsByUserIdInAProject(string userId, string projectId, int pageIndex, int pageSize);
        Task<Contract> GetContractByContractId(string id);
        long GenerateUniqueBookingCode();
        Task<Contract> ValidateContractOnSignedAsync(string id);
        Task UpdateSignedStatusForUserInContract(string contractId);
        Task<DocumentDownLoadResponseDTO> DownLoadFileContract(string userId, string contractId);
        Task<IEnumerable<Contract>> SearchContractWithFilters(ContractSearchDTO search, int pageIndex,
            int pageSize);
    }
}
