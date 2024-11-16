using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.EquityShare;
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
    public class ShareEquityService : IShareEquityService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IShareEquityRepository _shareEquityRepository;
        private readonly UserManager<User> _userManager;
        private readonly IContractRepository _contractRepository;
        private readonly IUserService _userService;

        public ShareEquityService(
            IContractRepository contractRepository, 
            IProjectRepository projectRepository, 
            IShareEquityRepository shareEquityRepository,
            UserManager<User> userManager,
            IUserService userService
        )
        {
            _contractRepository = contractRepository;
            _projectRepository = projectRepository;
            _userManager = userManager;
            _shareEquityRepository = shareEquityRepository;
            _userService = userService;
        }
        public async Task<List<ShareEquity>> GetShareEquityOfAllMembersInAProject(string userId, string projectId, EquityShareFilterDTO equityShareFilterDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var filterEquities = await _shareEquityRepository.GetShareEquityOfMembersInAProject(projectId);
            if (equityShareFilterDTO.FromDate.HasValue)
            {
                filterEquities = filterEquities.Where(x => x.DateAssigned >= equityShareFilterDTO.FromDate.Value);
            }
            if (equityShareFilterDTO.ToDate.HasValue)
            {
                filterEquities = filterEquities.Where(x => x.DateAssigned <= equityShareFilterDTO.ToDate.Value);
            }
            var newestShareEquity = await filterEquities
                .GroupBy(x => x.UserId)
                .Select(g => g.OrderByDescending(x => x.DateAssigned)
                .FirstOrDefault())
                .ToListAsync();
            return newestShareEquity;

        }
    }
}
