using StartedIn.CrossCutting.DTOs.RequestDTO.Phase;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services.Interface;

public interface IPhaseService
{
    Task<Phase> CreateNewPhase(string userId, string projectId, string charterId, CreatePhaseDTO createPhaseDto);
    Task<Phase> GetPhaseByPhaseId(string phaseId);
}