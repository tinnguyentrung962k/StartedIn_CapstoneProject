using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
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
    public class DealOfferService : IDealOfferService
    {
        private readonly IDealOfferRepository _dealOfferRepository;
        private readonly IDealOfferHistoryRepository _dealOfferHistoryRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<DealOffer> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProjectRepository _projectRepository;
        public DealOfferService(IDealOfferRepository dealOfferRepository, 
            IDealOfferHistoryRepository dealOfferHistoryRepository, 
            UserManager<User> userManager,
            ILogger<DealOffer> logger,
            IUnitOfWork unitOfWork,
            IProjectRepository projectRepository)
        {
            _dealOfferRepository = dealOfferRepository;
            _dealOfferHistoryRepository = dealOfferHistoryRepository;
            _userManager = userManager;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _projectRepository = projectRepository;
        }
        public async Task<DealOffer> SendADealOffer(string userId, DealOfferCreateDTO dealOfferCreateDTO)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                throw new NotFoundException("Không tìm thấy người dùng");
            }
            var project = await _projectRepository.GetOneAsync(dealOfferCreateDTO.ProjectId);
            if (project is null)
            {
                throw new NotFoundException("Không tìm thấy dự án");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                DealOffer dealOffer = new DealOffer
                {
                    InvestorId = userId,
                    Amount = dealOfferCreateDTO.Amount,
                    DealStatus = DealStatusEnum.Waiting,
                    ProjectId = dealOfferCreateDTO.ProjectId,
                    EquityShareOffer = dealOfferCreateDTO.EquityShareOffer,
                    Investor = user,
                    Project = project,
                    TermCondition = dealOfferCreateDTO.TermCondition,
                };
                var dealOfferEntity = _dealOfferRepository.Add(dealOffer);
                string notification = "Nhà đầu tư " + user.FullName + "đã gửi cho bạn lời mời đầu tư mới";
                DealOfferHistory dealOfferHistory = new DealOfferHistory
                {
                    Content = notification,
                    CreatedBy = user.FullName,
                    DealOffer = dealOffer,
                    DealOfferId = dealOffer.Id
                };
                var dealOfferHistoryEntity = _dealOfferHistoryRepository.Add(dealOfferHistory);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return dealOfferEntity;
            }
            catch (Exception ex) 
            {
                _logger.LogError($"An error occurred while creating a deal: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
