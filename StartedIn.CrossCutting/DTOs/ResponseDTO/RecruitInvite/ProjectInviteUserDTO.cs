using StartedIn.CrossCutting.DTOs.BaseDTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.RecruitInvite
{
    public class ProjectInviteUserDTO : IdentityResponseDTO
    {
        public string FullName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? StudentCode { get; set; }
        public string? IdCardNumber { get; set; }
    }
}
