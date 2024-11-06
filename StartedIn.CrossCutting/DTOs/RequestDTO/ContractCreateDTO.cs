using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class ContractCreateDTO
    {
        [Required(ErrorMessage = "Vui lòng điền tên hợp đồng")]
        [StringLength(50, ErrorMessage = "Tên hợp đồng không được quá 50 ký tự")]
        public string ContractName { get; set; }

        [Required(ErrorMessage = "Vui lòng điền các điều khoản khác")]
        public string ContractPolicy { get; set; }

        [Required(ErrorMessage = "Vui lòng điền số hợp đồng")]
        [StringLength(50, ErrorMessage = "Số hợp đồng không được quá 50 ký tự")]
        public string ContractIdNumber { get; set; }
    }
}
