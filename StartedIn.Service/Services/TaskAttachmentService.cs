using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.TaskAttachment;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services;

public class TaskAttachmentService : ITaskAttachmentService
{
    private readonly ITaskAttachmentRepository _taskAttachmentRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IAzureBlobService _azureBlobService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TaskAttachmentService> _logger;

    public TaskAttachmentService(ITaskAttachmentRepository taskAttachmentRepository, IAzureBlobService azureBlobService, IUnitOfWork unitOfWork,
        ILogger<TaskAttachmentService> logger, IProjectRepository projectRepository)
    {
        _taskAttachmentRepository = taskAttachmentRepository;
        _azureBlobService = azureBlobService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _projectRepository = projectRepository;
    }
    public async Task<TaskAttachment> AddTaskAttachment(string taskId, string projectId, TaskAttachmentCreateDTO taskAttachmentCreateDto)
    {
        var project = await _projectRepository.QueryHelper().Filter(p => p.Id.Equals(projectId)).GetOneAsync();
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }
        try
        {
            _unitOfWork.BeginTransaction();
            var fileUrl = await _azureBlobService.UploadTaskAttachment(taskAttachmentCreateDto.Attachment);
            var newAttachment = new TaskAttachment
            {
                TaskId = taskId,
                AttachmentUrl = fileUrl,
                FileName = Path.GetFileName(taskAttachmentCreateDto.Attachment.FileName)
            };
            var attachmentEntity = _taskAttachmentRepository.Add(newAttachment);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return attachmentEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating task attachment.");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteTaskAttachment(string taskAttachmentId)
    {
        var attachment = await _taskAttachmentRepository.QueryHelper().Filter(a => a.Id.Equals(taskAttachmentId))
            .GetOneAsync();
        if (attachment == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundTaskAttachmentError);
        }
        try
        {
            attachment.TaskId = null;
            await _taskAttachmentRepository.SoftDeleteById(taskAttachmentId);
            await _unitOfWork.SaveChangesAsync();
            //TODO: Delete file from Azure Blob Storage
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting task attachment");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}