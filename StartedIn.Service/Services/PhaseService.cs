using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services
{
    public class PhaseService : IPhaseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhaseRepository _phaseRepository;
        private readonly ILogger<Phase> _logger;

        public PhaseService(IUnitOfWork unitOfWork, IPhaseRepository phaseRepository, ILogger<Phase> logger)
        {
            _unitOfWork = unitOfWork;
            _phaseRepository = phaseRepository;
            _logger = logger;
        }

        public async Task<Phase> GetPhaseDetailById(string id)
        {
            var phase = await _phaseRepository.GetPhaseDetailById(id);
            if (phase == null)
            {
                throw new NotFoundException("Không có phase nào");
            }

            return phase;
        }
    }
}
