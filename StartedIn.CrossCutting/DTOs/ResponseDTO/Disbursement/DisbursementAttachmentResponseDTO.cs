using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Disbursement
{
    public class DisbursementAttachmentResponseDTO : IdentityResponseDTO
    {
        public string FileName { get; set; }
        public string EvidenceFile { get; set; }
    }
}
