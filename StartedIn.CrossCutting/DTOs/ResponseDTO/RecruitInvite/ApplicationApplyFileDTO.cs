using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.RecruitInvite
{
    public class ApplicationApplyFileDTO
    {
        public string Id { get; set; }
        public string ApplicationId { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
    }
}
