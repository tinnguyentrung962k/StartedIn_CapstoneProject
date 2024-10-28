using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class ContractUploadFileDTO
    {
        public string ContractId { get; set; }
        public IFormFile ContractFile { get; set; }
    }
}
