using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Recruitment
{
    public class RecruitmentImageCreateDTO
    {
        public IFormFile recruitFile { get; set; }
    }
}
