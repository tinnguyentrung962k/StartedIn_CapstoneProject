using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.TerminationRequest;
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
    public class TerminationRequestService : ITerminationRequestService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ITerminationRequestRepository _terminationRequestRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IContractRepository _contractRepository;
        private readonly UserManager<User> _userManager;
        private ILogger<TerminationRequestService> _logger;

        public TerminationRequestService(
            IProjectRepository projectRepository,
            ITerminationRequestRepository terminationRequestRepository,
            IUnitOfWork unitOfWork,
            IUserService userService,
            IContractRepository contractRepository,
            UserManager<User> userManager,
            ILogger<TerminationRequestService> logger)
        {
            _projectRepository = projectRepository;
            _terminationRequestRepository = terminationRequestRepository;
            _unitOfWork = unitOfWork;
            _userService = userService;
            _contractRepository = contractRepository;
            _userManager = userManager;
            _logger = logger;
        }
        public async Task CreateTerminationRequest(string userId, string projectId, string contractId, TerminationRequestCreateDTO requestCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var userInContract = await _userService.CheckIfUserBelongToContract(userId, contractId);
            if (userInContract.Contract.ContractStatus != CrossCutting.Enum.ContractStatusEnum.COMPLETED)
            {
                throw new InvalidDataException(MessageConstant.YouCannotRequestToTerminateThisContract);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var contract = await _contractRepository.GetContractById(contractId);
                var userParties = contract.UserContracts;
                var requestList = new List<TerminationRequest>();
                foreach (var userParty in userParties.Where(p => p.UserId != userId))
                {
                    var terminationRequest = new TerminationRequest
                    {
                        ContractId = contractId,
                        Contract = contract,
                        CreatedBy = userInProject.User.FullName,
                        FromId = userInProject.User.Id,
                        ToId = userParty.UserId,
                        Reason = requestCreateDTO.Reason
                    };
                    requestList.Add(terminationRequest);
                }
                await _terminationRequestRepository.AddRangeAsync(requestList);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error while creating request");
                await _unitOfWork.RollbackAsync();
                throw ex;
            }
        }
    }
}
