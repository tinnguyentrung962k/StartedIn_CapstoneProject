using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.DTOs.RequestDTO.Auth;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IUserService
    {
        Task<LoginResponseDTO> Login(string email, string password);

        Task Register(User user, string password);

        Task<string> Refresh(string refreshToken);

        Task Revoke(string userId);
        Task ActivateUser(string userId);
        Task<User> GetUserByUserName(string name);

        Task<User> GetUserWithUserRolesById(string userId);

        //Task<User> UpdateAvatar(IFormFile avatar, string userId);

        Task<User> UpdateProfile(User userToUpdate, string userId);

        //Task<User> UpdateCoverPhoto(IFormFile coverPhoto, string userId);

        Task<User> GetUserWithId(string id);
        Task<IEnumerable<User>> GetUsersList(int pageIndex, int pageSize);
        Task ImportUsersFromExcel(IFormFile file);
        Task RequestResetPassword(string email);
        Task ResetPassword(ResetPasswordDTO resetPasswordDTO);
        Task<UserProject> CheckIfUserInProject(string userId, string projectId);
        Task <UserContract> CheckIfUserBelongToContract(string userId, string contractId);

        //Task<IEnumerable<User>> GetUserSuggestedFriendList(string userId, int pageIndex, int pageSize);
    }
}
