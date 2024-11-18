using AutoMapper;
using Microsoft.AspNetCore.Identity;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.DealOffer;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        public TransactionService(
            ITransactionRepository transactionRepository, 
            IUserService userService, 
            UserManager<User> userManager,
            IMapper mapper)
        {
             _transactionRepository = transactionRepository;
            _userService = userService;
            _userManager = userManager;
            _mapper = mapper;
        }
        public async Task<PaginationDTO<TransactionResponseDTO>> GetListTransactionOfAProject(string userId, string projectId, int page, int size)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var transactions = _transactionRepository.QueryHelper()
                .Include(x => x.Finance)
                .Filter(x => x.Finance.ProjectId.Equals(projectId));
            var recordInPage = await transactions.GetPagingAsync(page, size);
            var response = _mapper.Map<List<TransactionResponseDTO>>(recordInPage);
            if (response.Any())
            {
                foreach (var transaction in response)
                {
                    var from = await _userService.GetUserWithId(recordInPage.First(t => t.Id == transaction.Id).FromID);
                    transaction.From = from.FullName;
                    var to = await _userService.GetUserWithId(recordInPage.First(t => t.Id == transaction.Id).ToID);
                    transaction.To = to.FullName;
                }
            }
            var pagination = new PaginationDTO<TransactionResponseDTO>
            {
                Data = response,
                Page = page,
                Size = size,
                Total = await transactions.GetTotal()

            };
            return pagination;


        }
    }
}
