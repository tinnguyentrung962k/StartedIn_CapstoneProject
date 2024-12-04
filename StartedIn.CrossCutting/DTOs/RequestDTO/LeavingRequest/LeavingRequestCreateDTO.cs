using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.LeavingRequest
{
    public class LeavingRequestCreateDTO
    {
        [Required(ErrorMessage = "Vui lòng điền lý do")]
        public string Reason { get; set; }
    }
}
