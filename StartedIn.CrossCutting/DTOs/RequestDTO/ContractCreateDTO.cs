using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class ContractCreateDTO
    {
        public string ProjectId { get; set; }
        public string ContractName { get; set; }
        public string ContractPolicy { get; set; }
    }
}
