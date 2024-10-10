using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class MilestoneInfoUpdateDTO
    {
        public string MilestoneTitle { get; set; }
        public string Description { get; set; }
        public DateOnly MilestoneDate { get; set; }
    }
}
