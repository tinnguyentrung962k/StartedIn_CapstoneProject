using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Project
{
    public class ProjectAdminFilterDTO
    {
        public string? ProjectName { get; set; }
        public string? Description { get; set; }
        public string? LeaderFullName { get; set; }
        public ProjectStatusEnum? Status { get; set; } 
    }
}
