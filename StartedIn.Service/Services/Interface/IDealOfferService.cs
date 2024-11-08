using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.RequestDTO.DealOffer;
using StartedIn.CrossCutting.DTOs.ResponseDTO.DealOffer;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface
{
    public interface IDealOfferService
    {
        Task<DealOffer> SendADealOffer(string userId, DealOfferCreateDTO dealOfferCreateDTO);
        Task <SearchResponseDTO<DealOfferForProjectResponseDTO>> GetDealOfferForAProject(string userId, string projectId,int pageIndex, int pageSize);
        Task<SearchResponseDTO<DealOfferForInvestorResponseDTO>> GetDealOfferForAnInvestor(string userId, int pageIndex, int pageSize);
        Task<DealOffer> AcceptADeal(string userId, string projectId, string dealId);
        Task<DealOffer> RejectADeal(string userId, string projectId, string dealId);
        Task<DealOffer> GetById(string id);
    }
}
