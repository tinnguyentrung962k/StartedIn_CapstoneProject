using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class ProjectCreateDTO
    {
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public IFormFile? LogoFile { get; set; }
        public int? TotalShares { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }
}
