using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.RequestDTO.Contract;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Contract;
using StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;


namespace StartedIn.Service.Services.Interface
{
    public interface IContractService
    {
        Task<Contract> CreateInvestmentContract(string userId, string projectId, InvestmentContractCreateDTO investmentContractCreateDTO);
        Task<Contract> SendSigningInvitationForContract(string projectId, string userId, string contractId);
        Task<Contract> GetContractByContractId(string userId, string id, string projectId);
        Task<Contract> ValidateContractOnSignedAsync(string id,string projectId);
        Task UpdateSignedStatusForUserInContract(string contractId, string projectId);
        Task<DocumentDownLoadResponseDTO> DownLoadFileContract(string userId, string projectId,string contractId);
        Task<PaginationDTO<ContractSearchResponseDTO>> SearchContractWithFilters(string userId, string projectId, ContractSearchDTO search, int page, int size);
        Task<Contract> CreateInvestmentContractFromDeal(string userId, string projectId, InvestmentContractFromDealCreateDTO investmentContractFromDealCreateDTO);
        Task<Contract> CreateStartupShareAllMemberContract(string userId, string projectId, GroupContractCreateDTO groupContractCreateDTO);
        Task<Contract> UpdateStartupShareAllMemberContract(string userId, string projectId, string contractId, GroupContractUpdateDTO groupContractUpdateDTO);
        Task<Contract> UpdateInvestmentContract(string userId, string projectId, string contractId, InvestmentContractUpdateDTO investmentContractUpdateDTO);
        Task<Contract> CancelContract(string userId, string projectId, string contractId);
        Task CancelContractAfterDueDate();
        Task MarkExpiredContract(string projectId, string contractId);
        Task DeleteContract(string userId, string projectId, string contractId);
        Task<List<UserContract>> GetUserSignHistoryInAContract(string userId, string projectId, string contractId);
        Task<Contract> CreateLiquidationNote(string userId, string projectId, string contractId, IFormFile uploadFile);
        Task LeaderTerminateContract(string userId, string projectId, string contractId, IFormFile uploadFile);
    }
}
