using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
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
        Task<Contract> CreateInvestmentContract(string userId, string projectId, InvestmentContractCreateDTO investmentContractCreateDTO);
        Task<Contract> SendSigningInvitationForContract(string projectId, string userId, string contractId);
        Task<Contract> GetContractByContractId(string userId, string id, string projectId);
        long GenerateUniqueBookingCode();
        Task<Contract> ValidateContractOnSignedAsync(string id,string projectId);
        Task UpdateSignedStatusForUserInContract(string contractId, string projectId);
        Task<DocumentDownLoadResponseDTO> DownLoadFileContract(string userId, string projectId,string contractId);
        Task<SearchResponseDTO<ContractSearchResponseDTO>> SearchContractWithFilters(string userId, string projectId, ContractSearchDTO search, int pageIndex, int pageSize);
        Task<Contract> UpdateInvestmentContract(string userId, string projectId, string contractId, InvestmentContractUpdateDTO investmentContractUpdateDTO);
    }
}
