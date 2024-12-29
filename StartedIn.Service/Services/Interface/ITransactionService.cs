using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.DTOs.RequestDTO.Transaction;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Transaction;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface ITransactionService
    {
        Task<PaginationDTO<TransactionResponseDTO>> GetListTransactionOfAProject(string userId, string projectId, TransactionFilterDTO transactionFilterDTO, int page, int size);
        Task<Transaction> AddAnTransactionForProject(string userId, string projectId, TransactionCreateDTO transactionCreateDTO);
        Task<TransactionResponseDTO> GetTransactionDetailById(string userId, string projectId, string transactionId);
        Task<Transaction> UploadEvidenceFile(string userId, string projectId, string transactionId, IFormFile file);
        Task<TransactionInAndOutMoneyDTO> GetInAndOutMoneyTransactionOfCurrentMonth(string projectId);
        Task<PaginationDTO<TransactionResponseDTO>> GetListTransactionOfUser(string userId, TransactionFilterDTO transactionFilterDTO, int page, int size);
        Task<float> CalculateProfitOfProject(string projectId);
        Task<float> CaluclateMonthProfitOfProject(string projectId);
    }
}
