using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Recruitment
{
    // This is only used to show the update interface of the recruitment post for members of the project
    public class RecruitmentInProjectDTO : IdentityResponseDTO
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsOpen { get; set; }
        public List<RecruitmentImgResponseDTO> RecruitmentImgs { get; set; }
    }
}
