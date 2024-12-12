using StartedIn.CrossCutting.DTOs.ResponseDTO.TerminationConfirmation;
using StartedIn.CrossCutting.DTOs.ResponseDTO.TerminationRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface ITerminationConfirmationService
    {
        Task<List<TerminationConfirmationResponseDTO>> GetTerminationConfirmationForUserInProject(string userId, string projectId);
        Task AcceptTerminationRequest(string userId, string projectId, string confirmId);
        Task RejectTerminationRequest(string userId, string projectId, string confirmId);

    }
}
