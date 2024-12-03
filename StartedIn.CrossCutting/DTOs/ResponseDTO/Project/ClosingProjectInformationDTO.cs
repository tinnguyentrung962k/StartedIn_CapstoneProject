using StartedIn.CrossCutting.DTOs.ResponseDTO.Asset;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Contract;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Project
{
    public class ClosingProjectInformationDTO
    {
        public decimal CurrentBudget { get; set; }
        public List<ContractInClosingProjectDTO> Contracts { get; set; }
        public List<DisbursementInClosingProjectDTO> Disbursements { get; set; }
        public List<AssetInClosingProjectDTO> Assets { get; set; }
    }
}
