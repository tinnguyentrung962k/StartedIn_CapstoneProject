using StartedIn.CrossCutting.DTOs.RequestDTO.TerminationRequest;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface ITerminationRequestService
    {
        Task CreateTerminationRequest(string userId, string projectId, string contractId, TerminationRequestCreateDTO requestCreateDTO);
    }
}
