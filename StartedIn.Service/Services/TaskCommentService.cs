using StartedIn.CrossCutting.DTOs.RequestDTO.TaskComment;
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
    public class TaskCommentService : ITaskCommentService
    {
        private readonly ITaskCommentRepository _taskCommentRepository;
        private readonly ITaskHistoryRepository _taskHistoryRepository;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;

        public TaskCommentService(ITaskCommentRepository taskCommentRepository, ITaskHistoryRepository taskHistoryRepository, IUserService userService, IUnitOfWork unitOfWork)
        {
            _taskCommentRepository = taskCommentRepository;
            _taskHistoryRepository = taskHistoryRepository;
            _userService = userService;
            _unitOfWork = unitOfWork;
        }

        public async Task<TaskComment> AddComment(string projectId, string taskId, string userId, TaskCommentCreateDTO taskCommentCreateDTO)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

            try
            {
                _unitOfWork.BeginTransaction();
                var taskComment = new TaskComment
                {
                    Content = taskCommentCreateDTO.Content,
                    UserId = userId,
                    CreatedTime = DateTime.Now,
                };
                _taskCommentRepository.Add(taskComment);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return taskComment;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }

        public Task<TaskComment> DeleteComment()
        {
            throw new NotImplementedException();
        }
    }
}
