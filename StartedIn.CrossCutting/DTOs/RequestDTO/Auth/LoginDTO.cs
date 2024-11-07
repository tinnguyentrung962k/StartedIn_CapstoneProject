using System.ComponentModel.DataAnnotations;

<<<<<<<< HEAD:StartedIn.CrossCutting/DTOs/RequestDTO/Authentication/LoginDTO.cs

namespace StartedIn.CrossCutting.DTOs.RequestDTO.Authentication
========
namespace StartedIn.CrossCutting.DTOs.RequestDTO.Auth
>>>>>>>> main:StartedIn.CrossCutting/DTOs/RequestDTO/Auth/LoginDTO.cs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; }
    }
}
