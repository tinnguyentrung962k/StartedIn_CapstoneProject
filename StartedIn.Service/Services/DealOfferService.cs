﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
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

        public async Task<SearchResponseDTO<DealOfferForProjectResponseDTO>> GetDealOfferForAProject(string userId,string projectId, int pageIndex, int pageSize)
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
            var record = await dealList.GetAllAsync();
            var totalRecord = record.Count();
            var deallistPaging = await dealList.GetPagingAsync(pageIndex, pageSize);
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
            var response = new SearchResponseDTO<DealOfferForProjectResponseDTO>
            {
                TotalRecord = totalRecord,
                PageIndex = pageIndex,
                PageSize = pageSize,
                ResponseList = dealInProjectResponse,
                TotalPage = (int)Math.Ceiling((double)totalRecord / pageSize)
            };
            return response;
            
        }
        public async Task<SearchResponseDTO<DealOfferForInvestorResponseDTO>> GetDealOfferForAnInvestor(string userId, int pageIndex, int pageSize)
        {
            var dealList = _dealOfferRepository.QueryHelper()
                .Include(x => x.Project)
                .Filter(x => x.InvestorId.Equals(userId));
            var record = await dealList.GetAllAsync();
            var totalRecord = record.Count();
            var deallistPaging = await dealList.GetPagingAsync(pageIndex, pageSize);
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
            var response = new SearchResponseDTO<DealOfferForInvestorResponseDTO>
            {
                TotalRecord = totalRecord,
                PageIndex = pageIndex,
                PageSize = pageSize,
                ResponseList = dealofInvestorResponse,
                TotalPage = (int)Math.Ceiling((double)totalRecord / pageSize)
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
    }
}
