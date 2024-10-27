using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO
{
    public class ContractResponseDTO : IdentityResponseDTO
    {
        public string ProjectId { get; set; }
        public string ContractName { get; set; }
        public string ContractType { get; set; }
        public string AttachmentLink { get; set; }
        public string ContractPolicy { get; set; }
        public DateOnly? ValidDate { get; set; }
        public DateOnly? ExpiredDate { get; set; }
        public List<UserInContractResponseDTO> userInContractResponseDTOs { get; set; }
        
    }
}
