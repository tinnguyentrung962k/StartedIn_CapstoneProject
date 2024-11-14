using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Disbursement
{
    public class DisbursementRejectDTO
    {
        [Required(ErrorMessage = "Vui lòng lý do từ chối giải ngân")]
        public string DeclineReason { get; set; }
    }
}
