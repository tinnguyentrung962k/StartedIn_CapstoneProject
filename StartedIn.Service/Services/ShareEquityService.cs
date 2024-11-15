using Microsoft.AspNetCore.Identity;
using StartedIn.CrossCutting.Constants;
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
        public async Task<List<ShareEquity>> GetShareEquityOfAllMembersInAProject(string userId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var shareEquities = await _shareEquityRepository.GetShareEquityOfMembersInAProject(projectId);
            if (shareEquities == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundShareEquityError);
            }
            return shareEquities;
            
        }
    }
}
