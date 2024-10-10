using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.DTOs.RequestDTO;
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
    public class MilestoneService : IMilestoneService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMilestoneRepository _milestoneRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IPhaseRepository _phaseRepository;
        private readonly ILogger<Milestone> _logger;

        public MilestoneService(
            IUnitOfWork unitOfWork,
            IMilestoneRepository milestoneRepository,
            ILogger<Milestone> logger,
            IPhaseRepository phaseRepository,
            ITaskRepository taskRepository)
        {
            _unitOfWork = unitOfWork;
            _milestoneRepository = milestoneRepository;
            _logger = logger;
            _phaseRepository = phaseRepository;
            _taskRepository = taskRepository;
        }
        public async Task<string> CreateNewMilestone(MilestoneCreateDTO milestoneCreateDto)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                Milestone milestone = new Milestone
                {
                    Position = milestoneCreateDto.Position,
                    PhaseId = milestoneCreateDto.PhaseId,
                    Title = milestoneCreateDto.MilstoneTitle,
                    Description = milestoneCreateDto.Description,
                    MilestoneDate = milestoneCreateDto.MilestoneDate,
                };
                var milestoneEntity = _milestoneRepository.Add(milestone);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return milestoneEntity.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating Milestone");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<Milestone> MoveMilestone(string milestoneId, string phaseId, int position, bool needsReposition)
        {
            var chosenMilestone = await _milestoneRepository.GetOneAsync(milestoneId);
            if (chosenMilestone == null)
            {
                throw new NotFoundException("Không có cột mốc được tìm thấy");
            }

            var chosenPhase = await _phaseRepository.QueryHelper()
                .Filter(p => p.Id.Equals(phaseId))
                .Include(p => p.Milestones)
                .GetOneAsync();
            if (chosenPhase == null)
            {
                throw new NotFoundException("Không có giai đoạn được tìm thấy");
            }

            var oldPhase = await _phaseRepository.GetOneAsync(chosenMilestone.PhaseId);
            if (oldPhase == null)
            {
                throw new NotFoundException("Không có giai đoạn cũ được tìm thấy");
            }
            if (oldPhase.ProjectId != chosenPhase.ProjectId)
            {
                throw new Exception("Giai đoạn cũ và giai đoạn mới không khớp");
            }

            try
            {

                // Update the chosen milestone's new phase information
                chosenMilestone.Position = position;
                chosenMilestone.PhaseId = phaseId;
                _milestoneRepository.Update(chosenMilestone);

                // Add the milestone to the new phase's task list
                chosenPhase.Milestones.Add(chosenMilestone);
                _phaseRepository.Update(chosenPhase);

                await _unitOfWork.SaveChangesAsync();

                if (needsReposition)
                {
                    _unitOfWork.BeginTransaction();

                    // Reposition tasks in the new phase
                    var newPhaseMilestones = await _milestoneRepository.QueryHelper()
                        .Filter(p => p.PhaseId.Equals(chosenPhase.Id))
                        .OrderBy(p => p.OrderBy(p => p.Position))
                        .GetAllAsync();

                    int newPhaseIncrement = (int)Math.Pow(2, 16);
                    int newPhaseCurrentPosition = (int)Math.Pow(2, 16);

                    foreach (var milestone in newPhaseMilestones)
                    {
                        milestone.Position = newPhaseCurrentPosition;
                        _milestoneRepository.Update(milestone);
                        newPhaseCurrentPosition += newPhaseIncrement;
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                }

                return chosenMilestone;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while moving major task");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public async Task<Milestone> GetMilestoneById(string id)
        {
            var milestone = await _milestoneRepository.GetMilestoneDetailById(id);
            if (milestone == null)
            {
                throw new NotFoundException("Không tìm thấy cột mốc");
            }
            return milestone;
        }

        public async Task<Milestone> UpdateMilestoneInfo(string id, MilestoneInfoUpdateDTO updateMilestoneInfoDTO)
        {
            var chosenMilestone = await _milestoneRepository.GetOneAsync(id);
            if (chosenMilestone == null)
            {
                throw new NotFoundException("Không tìm thấy cột mốc");
            }
            try
            {
                chosenMilestone.Title = updateMilestoneInfoDTO.MilestoneTitle;
                chosenMilestone.Description = updateMilestoneInfoDTO.Description;
                chosenMilestone.MilestoneDate = updateMilestoneInfoDTO.MilestoneDate;
                chosenMilestone.LastUpdatedTime = DateTimeOffset.UtcNow;
                _milestoneRepository.Update(chosenMilestone);
                await _unitOfWork.SaveChangesAsync();
                return chosenMilestone;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Failed while update task");
            }
        }
    }
}
