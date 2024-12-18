using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.Appointment;
using StartedIn.CrossCutting.Enum;
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
    
    public async Task<List<MeetingNote>> UploadMeetingNote(string userId, string projectId, string appointmentId,
        UploadMeetingNoteDTO uploadMeetingNoteDto)
    {
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        var appointment = await _appointmentRepository.QueryHelper().Filter(a => a.Id.Equals(appointmentId))
            .GetOneAsync();
        if (appointment == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundAppointment);
        }
        if (appointment.Status != MeetingStatus.Finished)
        {
            throw new CannotUploadMeetingNoteException(MessageConstant.CannotUploadMeetingNote);
        }

        var listResponse = new List<MeetingNote>();
        try
        { 
            _unitOfWork.BeginTransaction();
            foreach (var note in uploadMeetingNoteDto.Notes)
            {
                var noteUrl = await _azureBlobService.UploadMeetingNoteAndProjectDocuments(note);
                var meetingNote = new MeetingNote
                {
                    AppointmentId = appointmentId,
                    MeetingNoteLink = noteUrl,
                    FileName = note.FileName
                };
                var meetingNoteEntity = _meetingNoteRepository.Add(meetingNote);
                listResponse.Add(meetingNoteEntity);
            }
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return listResponse;
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