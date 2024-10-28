using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class DisbursementCreateDTO
    {
        public string DisbursementTitle { get; set; }
        public DateOnly DisbursementDate { get; set; }
        public decimal Amount { get; set; }
        public string Condition { get; set; }
    }
}
