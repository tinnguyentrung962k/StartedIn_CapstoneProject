using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.DTOs.RequestDTO.DealOffer;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.DealOffer;
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
        private readonly IUserService _userService;
        public DealOfferService(IDealOfferRepository dealOfferRepository, 
            IDealOfferHistoryRepository dealOfferHistoryRepository, 
            UserManager<User> userManager,
            ILogger<DealOffer> logger,
            IUnitOfWork unitOfWork,
            IProjectRepository projectRepository,
            IUserService userService)
        {
            _dealOfferRepository = dealOfferRepository;
            _dealOfferHistoryRepository = dealOfferHistoryRepository;
            _userManager = userManager;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _projectRepository = projectRepository;
            _userService = userService;
        }

        public async Task<PaginationDTO<DealOfferForProjectResponseDTO>> GetDealOfferForAProject(string userId, string projectId, int page, int size)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var project = await _projectRepository.GetProjectById(projectId);
            if (project == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }
            var dealList = _dealOfferRepository.QueryHelper()
                .Include(x => x.Investor)
                .Filter(x => x.ProjectId.Equals(projectId));
            var deallistPaging = await dealList.GetPagingAsync(page, size);
            List<DealOfferForProjectResponseDTO> dealInProjectResponse = new List<DealOfferForProjectResponseDTO>();
            foreach (var deal in deallistPaging)
            {
                DealOfferForProjectResponseDTO dealOfferForProjectResponseDTO = new DealOfferForProjectResponseDTO
                {
                    Id = deal.Id,
                    Amount = deal.Amount.ToString(),
                    DealStatus = deal.DealStatus,
                    EquityShareOffer = deal.EquityShareOffer.ToString(),
                    InvestorId = deal.InvestorId,
                    InvestorName = deal.Investor.FullName,
                    TermCondition = deal.TermCondition
                };
                dealInProjectResponse.Add(dealOfferForProjectResponseDTO);
            }
            var response = new PaginationDTO<DealOfferForProjectResponseDTO>
            {
                Data = dealInProjectResponse,
                Page = page,
                Size = size,
                Total = await dealList.GetTotal()

            };
            return response;
            
        }

        public async Task<PaginationDTO<DealOfferForInvestorResponseDTO>> GetDealOfferForAnInvestor(string userId, int page, int size)
        {
            var dealList = _dealOfferRepository.QueryHelper()
                .Include(x => x.Project)
                .Filter(x => x.InvestorId.Equals(userId));
            var deallistPaging = await dealList.GetPagingAsync(page, size);
            List<DealOfferForInvestorResponseDTO> dealofInvestorResponse = new List<DealOfferForInvestorResponseDTO>();
            foreach (var deal in deallistPaging)
            {
                var project = await _projectRepository.QueryHelper().Filter(x => x.Id.Equals(deal.ProjectId)).Include(x => x.UserProjects).GetOneAsync();
                User leader = null;
                foreach (var userProject in project.UserProjects)
                {
                    if (userProject.RoleInTeam == RoleInTeam.Leader)
                    {
                        var user = await _userManager.FindByIdAsync(userProject.UserId);
                        leader = user;
                    }
                }

                DealOfferForInvestorResponseDTO dealOfferForInvestorResponseDTO = new DealOfferForInvestorResponseDTO
                {
                    Id = deal.Id,
                    Amount = deal.Amount.ToString(),
                    DealStatus = deal.DealStatus,
                    EquityShareOffer = deal.EquityShareOffer.ToString(),
                    TermCondition = deal.TermCondition,
                    ProjectId = deal.ProjectId,
                    ProjectName = deal.Project.ProjectName,
                    LeaderId = leader.Id,
                    LeaderName = leader.FullName
                };
                dealofInvestorResponse.Add(dealOfferForInvestorResponseDTO);
            }
            var response = new PaginationDTO<DealOfferForInvestorResponseDTO>
            {
                Data = dealofInvestorResponse,
                Page = page,
                Size = size,
                Total = await dealList.GetTotal()
            };
            return response;

        }

        public async Task<DealOffer> SendADealOffer(string userId, DealOfferCreateDTO dealOfferCreateDTO)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                throw new NotFoundException(MessageConstant.NotFoundUserError);
            }
            var project = await _projectRepository.GetOneAsync(dealOfferCreateDTO.ProjectId);
            if (project is null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
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

        public async Task<DealOffer> AcceptADeal(string userId, string projectId, string dealId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId,projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var chosenDeal = await _dealOfferRepository.QueryHelper()
                .Include(x => x.Investor)
                .Filter(x => x.Id.Equals(dealId))
                .GetOneAsync();
            if (chosenDeal == null) {
                throw new NotFoundException(MessageConstant.NotFoundDealError);
            }
            if (chosenDeal.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.DealNotBelongToProjectError);
            }
            try
            {
                chosenDeal.DealStatus = DealStatusEnum.Accepted;
                _dealOfferRepository.Update(chosenDeal);
                await _unitOfWork.SaveChangesAsync();
                return chosenDeal;
            }
            catch (Exception ex) 
            {
                _logger.LogError($"An error occurred while update a deal: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public async Task<DealOffer> RejectADeal(string userId, string projectId, string dealId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var chosenDeal = await _dealOfferRepository.QueryHelper()
                .Include(x => x.Investor)
                .Filter(x=>x.Id.Equals(dealId))
                .GetOneAsync();
            if (chosenDeal == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundDealError);
            }
            if (chosenDeal.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.DealNotBelongToProjectError);
            }
            try
            {
                chosenDeal.DealStatus = DealStatusEnum.Rejected;
                _dealOfferRepository.Update(chosenDeal);
                await _unitOfWork.SaveChangesAsync();
                return chosenDeal;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while update a deal: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<DealOffer> GetById(string id)
        {
            var list = await _dealOfferRepository.Get(deal => deal.Id == id, null, "Investor");
            return list.FirstOrDefault() ?? throw new NotFoundException(MessageConstant.NotFoundDealError);
        }
        public async Task<DealOffer> GetDealOfferForInvestorById(string userId, string id)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundUserError);
            }
            var deal = await _dealOfferRepository.GetDealOfferById(id);
            if (deal is null)
            {
                throw new NotFoundException(MessageConstant.NotFoundDealError);
            }
            if (deal.InvestorId != userId)
            {
                throw new Exception(MessageConstant.NotFoundDealError);
            }
            return deal;
        }
    }
}
