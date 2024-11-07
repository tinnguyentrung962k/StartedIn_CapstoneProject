namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Authentication
{
    public class LoginResponseDTO
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
