using StartedIn.CrossCutting.DTOs.BaseDTO;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.LeavingRequest
{
    public class LeavingRequestResponseDTO : IdentityResponseDTO
    {
        public string UserId { get; set; }
        public string ProfilePicture { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public LeavingRequestStatus Status { get; set; }
        public string Reason { get; set; }
    }
}
