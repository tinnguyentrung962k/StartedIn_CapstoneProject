﻿using System.ComponentModel.DataAnnotations;

<<<<<<<< HEAD:StartedIn.CrossCutting/DTOs/RequestDTO/Authentication/ResetPasswordDTO.cs
namespace StartedIn.CrossCutting.DTOs.RequestDTO.Authentication
========
namespace StartedIn.CrossCutting.DTOs.RequestDTO.Auth
>>>>>>>> main:StartedIn.CrossCutting/DTOs/RequestDTO/Auth/ResetPasswordDTO.cs
{
    public class ResetPasswordDTO
    {
        [Required]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Mật khẩu phải lớn hơn 8 và nhỏ hơn 40 kí tự")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác thực không khớp")]
        public string ConfirmedPassword { get; set; }

        [Required]
        public string Token { get; set; }
    }
}