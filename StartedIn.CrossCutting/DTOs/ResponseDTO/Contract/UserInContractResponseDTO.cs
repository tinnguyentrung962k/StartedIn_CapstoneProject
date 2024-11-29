using StartedIn.CrossCutting.DTOs.BaseDTO;
namespace StartedIn.CrossCutting.DTOs.ResponseDTO.Contract
{
    public class UserInContractResponseDTO : IdentityResponseDTO
    {
        public string? ProfilePicture { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
