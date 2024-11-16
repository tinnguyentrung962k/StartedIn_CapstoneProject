using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.EquityShare
{
    public class EquityShareFilterDTO
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set;}
    }
}
