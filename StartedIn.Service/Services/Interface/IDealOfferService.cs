using DocumentFormat.OpenXml.Spreadsheet;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IDealOfferService
    {
        Task<DealOffer> SendADealOffer(string userId, DealOfferCreateDTO dealOfferCreateDTO);
        Task <SearchResponseDTO<DealOfferForProjectResponseDTO>> GetDealOfferForAProject(string userId, string projectId,int pageIndex, int pageSize);
        Task<SearchResponseDTO<DealOfferForInvestorResponseDTO>> GetDealOfferForAnInvestor(string userId, int pageIndex, int pageSize);
        Task<DealOffer> AcceptADeal(string userId, string projectId, string dealId);
        Task<DealOffer> RejectADeal(string userId, string projectId, string dealId);
    }
}
