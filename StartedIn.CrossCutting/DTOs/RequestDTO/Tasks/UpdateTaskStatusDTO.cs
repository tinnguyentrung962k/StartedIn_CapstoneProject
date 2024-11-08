using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Tasks
{
    public class UpdateTaskStatusDTO
    {
        public TaskEntityStatus Status { get; set; }
    }
}
