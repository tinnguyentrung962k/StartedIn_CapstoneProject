using StartedIn.CrossCutting.DTOs.RequestDTO.Phase;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IPhaseService
{
    Task<Phase> CreateNewPhase(string userId, string projectId, CreatePhaseDTO createPhaseDto);
    Task<Phase> GetPhaseByPhaseId(string projectId, string phaseId);
    Task<List<Phase>> GetPhasesByProjectId(string projectId);
}