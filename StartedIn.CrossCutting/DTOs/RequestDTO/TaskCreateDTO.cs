using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class TaskCreateDTO
    {
        public string TaskTitle { get; set; }
        public string Description { get; set; }
        public string TaskboardId { get; set; }
        public int Position { get; set; }
        public DateTimeOffset? Deadline { get; set; }
    }
}
