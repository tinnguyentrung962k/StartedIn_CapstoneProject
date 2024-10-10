using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class TaskboardCreateDTO
    {
        public int Position { get; set; }
        public string MilestoneId { get; set; }
        public string Title { get; set; }
    }
}
