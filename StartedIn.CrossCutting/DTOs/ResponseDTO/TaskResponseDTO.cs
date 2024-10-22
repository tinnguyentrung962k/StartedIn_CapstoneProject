using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class TaskResponseDTO : IdentityResponseDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public DateTimeOffset? Deadline { get; set; }
    }
}
