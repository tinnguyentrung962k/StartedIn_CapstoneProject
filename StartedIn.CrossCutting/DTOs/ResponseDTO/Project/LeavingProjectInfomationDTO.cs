using StartedIn.CrossCutting.DTOs.ResponseDTO.Contract;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
using StartedIn.CrossCutting.DTOs.ResponseDTO.LeavingRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Project
{
    public class LeavingProjectInfomationDTO
    {
        public List<ContractInClosingProjectDTO>? Contracts { get; set; }
        public List<DisbursementInClosingProjectDTO>? Disbursements { get; set; }
        public bool RequestExisted { get; set; }
    }
}
