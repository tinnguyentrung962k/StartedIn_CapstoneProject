using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.RecruitInvite
{
    public class ApplyRecruitmentDTO
    {
        public List<IFormFile> cvFiles { get; set; }
    }
}
