using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.RequestDTO
{
    public class RegisterDTO
    {
        [EmailAddress]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Mật khẩu phải lớn hơn 8 và nhỏ hơn 40 kí tự")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác thực không khớp")]
        public string? ConfirmedPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string? PhoneNumber { get; set; }

    }
}
