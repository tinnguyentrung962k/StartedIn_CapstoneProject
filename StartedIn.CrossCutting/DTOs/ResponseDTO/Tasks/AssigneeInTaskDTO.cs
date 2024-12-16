using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Tasks
{
    public class AssigneeInTaskDTO : IdentityResponseDTO
    {
        public string FullName { get; set; }
        public float? ActualManHour { get; set; }
        public string Email { get; set; }
    }
}
