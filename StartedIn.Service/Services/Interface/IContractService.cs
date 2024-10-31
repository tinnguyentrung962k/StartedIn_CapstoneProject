using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.DTOs.RequestDTO;
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
        Task<Contract> UploadContractFile(string userId, string contractId, IFormFile file);
        Task<IEnumerable<Contract>> GetContractsByUserIdInAProject(string userId, string projectId, int pageIndex, int pageSize);
        Task<Contract> GetContractByContractId(string id);
        long GenerateUniqueBookingCode();
        Task<Contract> ValidateContractOnSignedAsync(string id);
    }
}
