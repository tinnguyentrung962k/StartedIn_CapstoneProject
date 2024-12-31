using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.RequestDTO.DealOffer;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.DealOffer;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface
{
    public interface IDealOfferService
    {
        Task<DealOffer> SendADealOffer(string userId, DealOfferCreateDTO dealOfferCreateDTO);
        Task<PaginationDTO<DealOfferForProjectResponseDTO>> GetDealOfferForAProject(string userId, string projectId, int page, int size);
        Task<PaginationDTO<DealOfferForInvestorResponseDTO>> GetDealOfferForAnInvestor(string userId, int page, int size);
        Task<DealOffer> AcceptADeal(string userId, string projectId, string dealId);
        Task<DealOffer> RejectADeal(string userId, string projectId, string dealId);
        Task<DealOfferForProjectResponseDTO> GetById(string id);
        Task<DealOfferForInvestorResponseDTO> GetDealOfferForInvestorById(string userId, string id);
        Task DeleteADealOffer(string userId, string offerId);
    }
}
