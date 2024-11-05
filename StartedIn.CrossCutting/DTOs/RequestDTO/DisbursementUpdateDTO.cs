using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class DisbursementUpdateDTO : DisbursementCreateDTO
    {
        public string Id { get; set; }
    }
}
