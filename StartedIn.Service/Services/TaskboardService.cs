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
    public class TaskboardService : ITaskboardService
    {
        private readonly ITaskboardRepository _taskboardRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Taskboard> _logger;

        public TaskboardService(ITaskboardRepository taskboardRepository, IUnitOfWork unitOfWork, ILogger<Taskboard> logger)
        {
            _taskboardRepository = taskboardRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Taskboard> CreateNewTaskboard(TaskboardCreateDTO taskboardCreateDto)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                Taskboard taskboard = new Taskboard
                {
                    Position = taskboardCreateDto.Position,
                    MilestoneId = taskboardCreateDto.MilestoneId,
                    Title = taskboardCreateDto.Title
                };
                var taskboardEntity = _taskboardRepository.Add(taskboard);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return taskboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating Taskboard");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<Taskboard> MoveTaskBoard(string taskBoardId, int position, bool needsReposition)
        {
            var chosenTaskBoard = await _taskboardRepository.GetOneAsync(taskBoardId);
            if (chosenTaskBoard == null)
            {
                throw new NotFoundException("Không có bảng công việc được tìm thấy");
            }
            try
            {
                chosenTaskBoard.Position = position;
                _taskboardRepository.Update(chosenTaskBoard);
                await _unitOfWork.SaveChangesAsync();
                if (needsReposition)
                {
                    _unitOfWork.BeginTransaction();
                    // Get all milestones for the project and sort them by position
                    var taskBoards = await _taskboardRepository.QueryHelper()
                        .Filter(p => p.MilestoneId.Equals(chosenTaskBoard.MilestoneId))
                        .OrderBy(p => p.OrderBy(p => p.Position))
                        .GetAllAsync();

                    // Update positions starting from 2^16 (65536)
                    int increment = (int)Math.Pow(2, 16);
                    int currentPosition = (int)Math.Pow(2, 16);

                    foreach (var taskBoard in taskBoards)
                    {
                        taskBoard.Position = currentPosition;
                        _taskboardRepository.Update(taskBoard);
                        currentPosition += increment;
                        await _unitOfWork.SaveChangesAsync();
                    }
                    await _unitOfWork.CommitAsync();
                }
                return chosenTaskBoard;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while moving task board");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<Taskboard> GetTaskboardById(string id)
        {
            var taskboard = await _taskboardRepository.QueryHelper()
                .Filter(t => t.Id.Equals(id))
                .Include(t => t.TasksList.OrderBy(mt => mt.Position))
                .GetOneAsync();
            if (taskboard == null)
            {
                throw new NotFoundException("Không có taskboard nào");
            }
            return taskboard;
        }

        public async Task<Taskboard> UpdateTaskboard(string id, TaskboardInfoUpdateDTO taskboardInfoUpdateDTO)
        {
            var taskboard = await _taskboardRepository.GetOneAsync(id);
            if (taskboard == null)
            {
                throw new NotFoundException("Không tìm thấy bảng công việc");
            }
            try
            {
                taskboard.Title = taskboardInfoUpdateDTO.Title;
                taskboard.LastUpdatedTime = DateTimeOffset.UtcNow;
                _taskboardRepository.Update(taskboard);
                await _unitOfWork.SaveChangesAsync();
                return taskboard;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception("Failed while update taskboard");
            }

        }
    }
}
