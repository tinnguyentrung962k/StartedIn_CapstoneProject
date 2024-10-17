using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.Enum;
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

        public async Task<Phase> CreateNewPhase(PhaseCreateDTO phaseCreateDto)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                Phase phase = new Phase();
                switch (phaseCreateDto.ChosenPhase)
                {
                    case PhaseEnum.Initializing:
                    {
                        phase.PhaseName = "Khởi động";
                        break;
                    }
                    case PhaseEnum.Planning:
                    {
                        phase.PhaseName = "Lập kế hoạch";
                        break;
                    }
                    case PhaseEnum.Executing:
                    {
                        phase.PhaseName = "Triển khai";
                        break;
                    }
                    case PhaseEnum.Monitoring:
                    {
                        phase.PhaseName = "Giám sát";
                        break;
                    }
                    case PhaseEnum.Closing:
                    {
                       phase.PhaseName = "Kết thúc";
                       break;
                    }

                }
                phase.ProjectId = phaseCreateDto.ProjectId;
                phase.StartDate = phaseCreateDto.StartDate;
                phase.EndDate = phaseCreateDto.EndDate;
                DateTime startDateTime = phase.StartDate.ToDateTime(TimeOnly.MinValue);
                DateTime endDateTime = phase.EndDate.ToDateTime(TimeOnly.MinValue);
                TimeSpan duration = endDateTime - startDateTime;
                phase.Duration = (int)duration.TotalDays;
                var phaseEntity = _phaseRepository.Add(phase);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return phaseEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating phase");
                await _unitOfWork.RollbackAsync();
                throw;
            }
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
