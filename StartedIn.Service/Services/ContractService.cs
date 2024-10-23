using Microsoft.Extensions.Logging;
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
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Contract> _logger;
        public ContractService(IContractRepository contractRepository, 
            IUnitOfWork unitOfWork, 
            ILogger<Contract> logger)
        {
            _contractRepository = contractRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
    }
}
