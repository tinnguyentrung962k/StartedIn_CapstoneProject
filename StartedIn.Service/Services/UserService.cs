using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using StartedIn.Repository.Repositories.Extensions;
using OfficeOpenXml;
using System;
using StartedIn.Repository.Repositories;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Authentication;
using StartedIn.CrossCutting.DTOs.RequestDTO.Auth;
using StartedIn.CrossCutting.Enum;

namespace StartedIn.Service.Services
{
    public class UserService : IUserService
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;
        private readonly RoleManager<Role> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IProjectRepository _projectRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IUserRepository _userRepository;
        public UserService(ITokenService tokenService,
            UserManager<User> userManager, IUnitOfWork unitOfWork,
            IConfiguration configuration, IEmailService emailService,
            ILogger<UserService> logger, IHttpContextAccessor httpContextAccessor,
            RoleManager<Role> roleManager,
            IProjectRepository projectRepository,IContractRepository contractRepository,
            IUserRepository userRepository
        )
        {
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _userManager = userManager;
            _logger = logger;
            _roleManager = roleManager;
            _configuration = configuration;
            _projectRepository = projectRepository;
            _contractRepository = contractRepository;
            _userRepository = userRepository;
        }

        public async Task<LoginResponseDTO> Login(string email, string password)
        {
            var loginUser = await _userManager.FindByEmailAsync(email);
            if (loginUser == null)
            {
                throw new InvalidLoginException("Sai tài khoản đăng nhập");
            }
            if (!await _userManager.CheckPasswordAsync(loginUser, password))
            {
                throw new InvalidLoginException("Sai mật khẩu đăng nhập");
            }
            if (loginUser.EmailConfirmed == false)
            {
                throw new NotActivateException("Tài khoản chưa được xác thực");
            }
            var user = await _userManager.FindByIdAsync(loginUser.Id);
            string jwtToken;
            string refreshToken;
            if (user.RefreshToken != null)
            {
                jwtToken = _tokenService.CreateTokenForAccount(loginUser);
                return new LoginResponseDTO()
                {
                    AccessToken = jwtToken,
                    RefreshToken = user.RefreshToken
                };
            }
            else
            {
                jwtToken = _tokenService.CreateTokenForAccount(loginUser);
                refreshToken = _tokenService.GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                await _userManager.UpdateAsync(user);
            }
            return new LoginResponseDTO
            {
                AccessToken = jwtToken,
                RefreshToken = refreshToken,
            };
        }


        public async Task Register(User registerUser, string password)
        {
            var existUser = await _userManager.FindByEmailAsync(registerUser.Email);
            if (existUser != null)
            {
                throw new ExistedEmailException("Email này đã tồn tại.");
            }

            try
            {
                _unitOfWork.BeginTransaction();
                registerUser.UserName = registerUser.Email;
                registerUser.ProfilePicture = ProfileConstant.defaultAvatarUrl;
                var result = await _userManager.CreateAsync(registerUser, password);
                if (!result.Succeeded)
                {
                    throw new InvalidRegisterException("Đăng ký thất bại");
                }
                await _userManager.AddToRoleAsync(registerUser, RoleConstants.INVESTOR);
                await _unitOfWork.SaveChangesAsync();
                // Only send mail if user is created successfully
                _emailService.SendVerificationMailAsync(registerUser.Email, registerUser.Id);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while register");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<string> Refresh(string refreshToken)
        {
            var user = await _userManager.FindRefreshTokenAsync(refreshToken);
            if (user == null || !refreshToken.Equals(user.RefreshToken))
            {
                throw new NotFoundException("Không tìm thấy người dùng!");
            }
            var jwtToken = _tokenService.CreateTokenForAccount(user);
            return jwtToken;

        }

        public async Task Revoke(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng!");
            }
            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);
        }

        public async Task ActivateUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException($"Unable to activate user {userId}");
            }
            user.EmailConfirmed = true;
            user.Verified = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);
        }
        public async Task<User> GetUserByUserName(string name)
        {
            var user = await _userManager.FindByNameAsync(name);
            return user;
        }

        public async Task<User> GetUserWithUserRolesById(string userId)
        {
            return await _userManager.Users
                .Include(it => it.UserRoles)
                .ThenInclude(r => r.Role)
                .SingleOrDefaultAsync(it => it.Id == userId);
        }

        //public virtual async Task<User> UpdateAvatar(IFormFile avatar, string userId)
        //{
        //    var url = await _azureBlobService.UploadAvatarOrCover(avatar);
        //    var user = await GetUserWithId(userId);
        //    user.ProfilePicture = url;
        //    await _userManager.UpdateAsync(user);
        //    return user;
        //}

        public virtual async Task<User> UpdateProfile(User userToUpdate, string userId)
        {
            var user = await GetUserWithId(userId);
            user.Bio = userToUpdate.Bio;
            user.PhoneNumber = userToUpdate.PhoneNumber;
            await _userManager.UpdateAsync(user);
            return user;
        }

        //public virtual async Task<User> UpdateCoverPhoto(IFormFile coverPhoto, string userId)
        //{
        //    var url = await _azureBlobService.UploadAvatarOrCover(coverPhoto);
        //    var user = await GetUserWithId(userId);
        //    user.CoverPhoto = url;
        //    await _userManager.UpdateAsync(user);
        //    return user;
        //}

        public async Task<User> GetUserWithId(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return user;
        }

        public async Task<IEnumerable<User>> GetUsersList(int pageIndex, int pageSize)
        {
            var userList = await _userManager.GetUsersAsync(pageIndex, pageSize);
            if (!userList.Any())
            {
                throw new NotFoundException("Không có người dùng nào trong danh sách");
            }
            return userList;
        }
        public async Task ImportUsersFromExcel(IFormFile file)
        {
            // Check if file is valid
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var newUsers = new List<(User user, string password)>(); // Tuple to store user and password

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the license context
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0]; // Assume first sheet

                        // Get the column names from the first row
                        var columnNames = new Dictionary<string, int>();
                        var columnCount = worksheet.Dimension.Columns;

                        for (int col = 1; col <= columnCount; col++)
                        {
                            var columnName = worksheet.Cells[1, col].Text; // Reading the first row for column names
                            if (!string.IsNullOrEmpty(columnName))
                            {
                                columnNames[columnName] = col; // Map column name to index
                            }
                        }

                        // Ensure required columns exist
                        if (!columnNames.ContainsKey("Email") || !columnNames.ContainsKey("FullName"))
                        {
                            throw new ArgumentException("The Excel file must contain 'Email' and 'FullName' columns.");
                        }

                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++) // Skip header row
                        {
                            // Get data by column names
                            var email = worksheet.Cells[row, columnNames["Email"]].Text;
                            var fullName = worksheet.Cells[row, columnNames["FullName"]].Text;
                            var password = GenerateRandomPassword(8); // Generate random password
                            var studentCode = worksheet.Cells[row, columnNames["StudentID"]].Text;
                            var phoneNumber = worksheet.Cells[row, columnNames["PhoneNumber"]].Text;
                            // Skip rows with missing data
                            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                            {
                                continue;
                            }

                            // Check if user already exists
                            var existingUser = await _userManager.FindByEmailAsync(email);
                            if (existingUser != null)
                            {
                                // Log or continue to skip this user
                                _logger.LogInformation($"User with email {email} already exists. Skipping.");
                                continue; // Skip this user and move to the next row
                            }

                            // Create new user
                            var newUser = new User
                            {
                                Email = email,
                                UserName = email,
                                FullName = fullName,
                                EmailConfirmed = true, // Set default, adjust as needed
                                ProfilePicture = ProfileConstant.defaultAvatarUrl, // Default avatar
                                PhoneNumber = phoneNumber,
                                StudentCode = studentCode
                            };

                            var result = await _userManager.CreateAsync(newUser, password);
                            if (result.Succeeded)
                            {
                                // Assign default role after successful creation
                                await _userManager.AddToRoleAsync(newUser, RoleConstants.USER);
                                newUsers.Add((newUser, password)); // Store new user and password in the list
                            }
                            else
                            {
                                // Handle failure case
                                throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                            }
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                foreach (var (user, password) in newUsers) // Loop through the newly created users
                {
                    // Send account info email including the email and generated password
                    _emailService.SendAccountInfoMailAsync(user.Email, password);
                }

                await _unitOfWork.CommitAsync(); // Commit the transaction after sending the emails
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during user import: {ex.Message}");
                _unitOfWork.RollbackAsync();
                throw;
            }
        }
        private string GenerateRandomPassword(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task RequestResetPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                throw new NotFoundException("Người dùng không tồn tại");
            }

            // Generate reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Get app domain from configuration
            var appDomain = _configuration.GetValue<string>("API_DOMAIN"); // You should add this to your config

            // Manually construct reset URL
            var resetLink = $"{appDomain}/account/reset-password?token={token}&email={user.Email}";

            // Send reset email
            await _emailService.SendResetPasswordEmail(user.Email, resetLink);
        }

        public async Task ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDTO.Email);
            if (user is null)
            {
                throw new NotFoundException(MessageConstant.NotFoundUserError);
            }
            var resetResult = await _userManager.ResetPasswordAsync(user, resetPasswordDTO.Token, resetPasswordDTO.NewPassword);
            if (!resetResult.Succeeded)
            {
                // Handle the error (log and return validation errors)
                foreach (var error in resetResult.Errors)
                {
                    throw new Exception(error.ToString());
                }
            }
        }
        public async Task<UserProject> CheckIfUserInProject(string userId, string projectId)
        {
            var user = await _userManager.Users
                .Include(u => u.UserProjects) // Ensure UserProjects are loaded
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                throw new NotFoundException(MessageConstant.NotFoundUserError);
            }

            var project = await _projectRepository.GetOneAsync(projectId);
            if (project is null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }

            var isUserInProject = await _userRepository.CheckIfUserInProject(userId, project.Id);
            if (isUserInProject is false)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.UserNotInProjectError);
            }

            // Null-check user.UserProjects before accessing
            var userRoleInProject = await _projectRepository.GetUserRoleInProject(userId, projectId);

            var userProject = new UserProject
            {
                UserId = userId,
                ProjectId = projectId,
                User = user,
                Project = project,
                RoleInTeam = userRoleInProject
            };
            return userProject;
        }
        public async Task<UserContract> CheckIfUserBelongToContract(string userId, string contractId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                throw new NotFoundException(MessageConstant.NotFoundUserError);
            }
            var chosenContract = await _contractRepository.GetContractById(contractId);
            if (chosenContract is null)
            {
                throw new NotFoundException(MessageConstant.NotFoundContractError);
            }
            var isUserBelongToContract = await _userRepository.IsUserBelongToAContract(userId, contractId);
            if (isUserBelongToContract is false)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.UserNotBelongContractError);
            }
            var userInContract = new UserContract
            {
                UserId = userId,
                ContractId = contractId,
                User = user,
                Contract = chosenContract,
            };
            return userInContract;
        }
        public async Task<bool> IsUserInProject(string userId, string projectId)
        {
            return await _userRepository.CheckIfUserInProject(userId, projectId);
        }
    }
}
