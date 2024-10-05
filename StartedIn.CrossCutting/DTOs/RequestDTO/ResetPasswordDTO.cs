using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class ResetPasswordDTO 
    {
        [Required]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters and less than 40 characters long.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
