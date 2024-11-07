using System.ComponentModel.DataAnnotations;


namespace StartedIn.CrossCutting.DTOs.RequestDTO.Auth
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; }
    }
}
