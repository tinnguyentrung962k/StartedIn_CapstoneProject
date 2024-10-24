using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class UserInContractResponseDTO : IdentityResponseDTO
    {
        public string PartyFullName { get; set; }
    }
}
