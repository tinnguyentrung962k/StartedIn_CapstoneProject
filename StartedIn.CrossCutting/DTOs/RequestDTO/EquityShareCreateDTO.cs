using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class EquityShareCreateDTO
    {
        public string UserId { get; set; }
        public int? ShareQuanity { get; set; }
        public int? Percentage { get; set; }
        public string? StakeholderType { get; set; }
    }
}
