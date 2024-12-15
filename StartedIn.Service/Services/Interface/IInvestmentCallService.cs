using StartedIn.CrossCutting.DTOs.RequestDTO.EquityShare;
using StartedIn.CrossCutting.DTOs.RequestDTO.InvestmentCall;
using StartedIn.CrossCutting.DTOs.ResponseDTO.InvestmentCall;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IInvestmentCallService 
{
    Task<InvestmentCall> CreateNewInvestmentCall(string userId, string projectId, InvestmentCallCreateDTO investmentCallCreateDto);

    Task<InvestmentCall> GetInvestmentCallById(string projectId, string investmentCallId);
    Task<PaginationDTO<InvestmentCallResponseDTO>> GetInvestmentCallByProjectId(string userId, string projectId, InvestmentCallSearchDTO investmentCallSearchDTO, int page, int size);
}