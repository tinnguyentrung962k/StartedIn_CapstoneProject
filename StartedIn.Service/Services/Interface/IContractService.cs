using StartedIn.CrossCutting.Customize;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IContractService
    {
        Task<Contract> CreateAContract(string userId, ContractCreateDTO contractCreateDTO, List<EditableField> editableFields);
    }
}
