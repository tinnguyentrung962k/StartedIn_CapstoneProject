using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Contract
{
    public class UserInContractHistoryResponseDTO
    {
        public string ProfilePicture { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTimeOffset? SignedDate { get; set; }
        public bool IsReject { get; set; }
    }
}
