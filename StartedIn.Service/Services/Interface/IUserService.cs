using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.DTOs.RequestDTO.Auth;
using StartedIn.CrossCutting.DTOs.RequestDTO.User;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Authentication;
using StartedIn.Domain.Entities;


namespace StartedIn.Service.Services.Interface
{
    public interface IUserService
    {
        Task<LoginResponseDTO> Login(string email, string password);

        Task Register(User registerUser, string password, string role);

        Task<string> Refresh(string refreshToken);

        Task Revoke(string userId);
        Task ActivateUser(string userId);
        Task<User> GetUserByUserName(string name);

        Task<User> GetUserWithUserRolesById(string userId);

        //Task<User> UpdateAvatar(IFormFile avatar, string userId);

        Task<User> UpdateProfile(User userToUpdate, string userId);

        //Task<User> UpdateCoverPhoto(IFormFile coverPhoto, string userId);

        Task<User> GetUserWithId(string id);
        Task<PaginationDTO<FullProfileDTO>> GetUsersListForAdmin(UserAdminFilterDTO userAdminFilterDTO,int page, int size);
        Task ImportUsersFromExcel(IFormFile file);
        Task RequestResetPassword(string email);
        Task ResetPassword(ResetPasswordDTO resetPasswordDTO);
        Task<UserProject> CheckIfUserInProject(string userId, string projectId);
        Task <UserContract> CheckIfUserBelongToContract(string userId, string contractId);
        Task<bool> IsUserInProject(string userId, string projectId);
        Task ToggleUserStatus(string userId);
        Task<ICollection<UserProject>> GetProjectsByUserId(string userId);

        //Task<IEnumerable<User>> GetUserSuggestedFriendList(string userId, int pageIndex, int pageSize);
    }
}
