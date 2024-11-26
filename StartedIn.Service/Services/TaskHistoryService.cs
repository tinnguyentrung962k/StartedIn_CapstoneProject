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

        public TaskHistoryService(ITaskHistoryRepository taskHistoryRepository, IUserService userService)
        {
            _taskHistoryRepository = taskHistoryRepository;
            _userService = userService;
        }

        public async Task<IEnumerable<TaskHistory>> GetTaskHistoryOfTask(string projectId, string taskId, string userId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var taskHistory = await _taskHistoryRepository.QueryHelper().Filter(th => th.TaskId.Equals(taskId)).OrderBy(x => x.OrderBy(x => x.CreatedTime)).GetAllAsync();

            return taskHistory;
        }
    }
}
