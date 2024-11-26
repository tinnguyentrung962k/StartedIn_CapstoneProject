using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement
{
    public class DisbursementOverviewOfProjectForInvestor : IdentityResponseDTO
    {
        public string ProjectName { get; set; }
        public string LogoUrl { get; set; }
        public List<DisbursementOverviewOfProject> DisbursementInfo { get; set; }
    }
}
