using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Tasks
{
    public class UpdateTaskStatusDTO
    {
        [Required]
        public TaskEntityStatus Status { get; set; }
    }
}
