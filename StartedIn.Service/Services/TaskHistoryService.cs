using AutoMapper;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TaskHistory;
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
    public class TaskHistoryService : ITaskHistoryService
    {
        private readonly ITaskHistoryRepository _taskHistoryRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public TaskHistoryService(ITaskHistoryRepository taskHistoryRepository, IUserService userService, IMapper mapper)
        {
            _taskHistoryRepository = taskHistoryRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<PaginationDTO<TaskHistoryResponseDTO>> GetTaskHistoriesOfProject(string projectId, int page, int size)
        {
            var taskHistories = await _taskHistoryRepository.QueryHelper().Filter(t => t.Task.ProjectId.Equals(projectId))
                .OrderBy(t => t.OrderByDescending(t => t.CreatedTime)).GetPagingAsync(page, size);
            return new PaginationDTO<TaskHistoryResponseDTO>()
            {
                Data = _mapper.Map<IEnumerable<TaskHistoryResponseDTO>>(taskHistories),
                Page = page,
                Size = size,
                Total = await _taskHistoryRepository.QueryHelper().Filter(t => t.Task.ProjectId.Equals(projectId)).GetTotal(),
            };
        }

        public async Task<IEnumerable<TaskHistory>> GetTaskHistoryOfTask(string projectId, string taskId, string userId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var taskHistory = await _taskHistoryRepository.QueryHelper().Filter(th => th.TaskId.Equals(taskId)).OrderBy(x => x.OrderByDescending(x => x.CreatedTime)).GetAllAsync();

            return taskHistory;
        }
    }
}
