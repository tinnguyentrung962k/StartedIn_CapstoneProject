using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.TerminationRequest
{
    public class TerminationRequestCreateDTO
    {
        [Required(ErrorMessage = "Vui lòng điền lý do huỷ hợp đồng")]
        public string Reason { get; set; }
    }
}
