using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services;

public class MeetingNoteService : IMeetingNoteService
{
    private readonly IAzureBlobService _azureBlobService;
    private readonly IMeetingNoteRepository _meetingNoteRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectRepository _projectRepository;
    public MeetingNoteService(IAzureBlobService azureBlobService, IMeetingNoteRepository meetingNoteRepository, IUserService userService, IUnitOfWork unitOfWork,
        IProjectRepository projectRepository, IAppointmentRepository appointmentRepository)
    {
        _azureBlobService = azureBlobService;
        _meetingNoteRepository = meetingNoteRepository;
        _userService = userService;
        _unitOfWork = unitOfWork;
        _projectRepository = projectRepository;
        _appointmentRepository = appointmentRepository;
    }
    
    public async Task<MeetingNote> UploadMeetingNote(string userId, string projectId, string appointmentId,
        UploadMeetingNoteDTO uploadMeetingNoteDto)
    {
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        try
        {
            var noteUrl = await _azureBlobService.UploadMeetingNote(uploadMeetingNoteDto.Note);
            var meetingNote = new MeetingNote
            {
                AppointmentId = appointmentId,
                MeetingNoteLink = noteUrl
            };
            var meetingNoteEntity = _meetingNoteRepository.Add(meetingNote);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return meetingNoteEntity;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw new Exception("Failed while creating meeting note.");
        }
            
    }

    public async Task<MeetingNote> GetMeetingNoteById(string projectId, string appointmentId, string meetingNoteId)
    {
        var project = await _projectRepository.GetProjectById(projectId);
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }
        var meetingNote = await _meetingNoteRepository.QueryHelper()
            .Filter(mn => mn.Id.Equals(meetingNoteId))
            .GetOneAsync();
        if (meetingNote == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundMeetingNote);
        }
        return meetingNote;
    }

    public async Task<List<MeetingNote>> GetMeetingNoteByAppointmentId(string projectId, string appointmentId)
    {
        var project = await _projectRepository.GetProjectById(projectId);
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }
        var meetingNotes = await _meetingNoteRepository.QueryHelper()
            .Filter(mn => mn.AppointmentId.Equals(appointmentId))
            .GetAllAsync();
        if (meetingNotes == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundMeetingNote);
        }
        return meetingNotes.ToList();
    }
}