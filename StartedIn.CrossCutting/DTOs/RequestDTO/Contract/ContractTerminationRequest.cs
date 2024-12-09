using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Contract
{
    public class ContractTerminationRequest
    {
        [Required(ErrorMessage = "Vui lòng điền lý do thanh lý hợp đồng")]
        public string TerminationReason { get; set; }
    }
}
