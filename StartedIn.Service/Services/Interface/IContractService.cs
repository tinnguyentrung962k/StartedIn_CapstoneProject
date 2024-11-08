using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.RequestDTO.Contract;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Contract;
using StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO;
using StartedIn.Domain.Entities;


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
        Task<Contract> CreateInvestmentContractFromDeal(string userId, string projectId, InvestmentContractFromDealCreateDTO investmentContractFromDealCreateDTO);
        Task<Contract> CreateStartupShareAllMemberContract(string userId, string projectId, GroupContractCreateDTO groupContractCreateDTO);
        Task<Contract> CancelContract(string userId, string projectId, string contractId);
    }
}
