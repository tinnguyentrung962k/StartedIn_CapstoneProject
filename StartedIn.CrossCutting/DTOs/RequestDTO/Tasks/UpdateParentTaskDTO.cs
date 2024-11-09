using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Tasks
{
    public class UpdateParentTaskDTO
    {
        [Required]
        public string ParentTaskId { get; set; }
    }
}
