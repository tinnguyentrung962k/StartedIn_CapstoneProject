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
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using AutoMapper;
using StartedIn.CrossCutting.DTOs.RequestDTO.User;

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
        private readonly IMapper _mapper;
        public UserService(ITokenService tokenService,
            UserManager<User> userManager, IUnitOfWork unitOfWork,
            IConfiguration configuration, IEmailService emailService,
            ILogger<UserService> logger, IHttpContextAccessor httpContextAccessor,
            RoleManager<Role> roleManager,
            IProjectRepository projectRepository, IContractRepository contractRepository,
            IUserRepository userRepository,
            IMapper mapper
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
            _mapper = mapper;
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
            if (loginUser.IsActive == false)
            {
                throw new NotActivateException("Tài khoản đã bị vô hiệu hoá");
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


        public async Task Register(User registerUser, string password, string role)
        {
            var existUser = await _userManager.FindByEmailAsync(registerUser.Email);
            if (existUser != null)
            {
                throw new ExistedEmailException("Email này đã tồn tại.");
            }

            if (role == RoleConstants.USER && !registerUser.Email.EndsWith("@fpt.edu.vn"))
            {
                throw new InvalidRegisterException("Email của sinh viên phải có đuôi @fpt.edu.vn.");
            }

            try
            {
                _unitOfWork.BeginTransaction();
                registerUser.UserName = registerUser.Email;
                registerUser.ProfilePicture = ProfileConstant.defaultAvatarUrl;

                var result = await _userManager.CreateAsync(registerUser, password);
                if (!result.Succeeded)
                {
                    throw new InvalidRegisterException("Đăng ký thất bại.");
                }

                await _userManager.AddToRoleAsync(registerUser, role);
                await _unitOfWork.SaveChangesAsync();

                // Only send mail if user is created successfully
                await _emailService.SendVerificationMailAsync(registerUser.Email, registerUser.Id);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering.");
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
            if (user.EmailConfirmed == true)
            {
                throw new ActivateException(MessageConstant.AccountAlreadyActivate);
            }
            user.EmailConfirmed = true;
            user.IsActive = true;
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

        public async Task<PaginationDTO<FullProfileDTO>> GetUsersListForAdmin(UserAdminFilterDTO userAdminFilterDTO, int page, int size)
        {
            var userListQuery = _userRepository.GetUsersInTheSystemQuery()
                .Where(x => x.UserRoles.Any(u => u.RoleId != "role_admin"));
            if (!string.IsNullOrWhiteSpace(userAdminFilterDTO.FullName))
            {
                userListQuery = userListQuery.Where(x => x.FullName.ToLower().Contains(userAdminFilterDTO.FullName.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(userAdminFilterDTO.Email))
            {
                userListQuery = userListQuery.Where(x => x.Email.ToLower().Contains(userAdminFilterDTO.Email.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(userAdminFilterDTO.PhoneNumber))
            {
                userListQuery = userListQuery.Where(x => x.PhoneNumber.ToLower().Contains(userAdminFilterDTO.PhoneNumber.ToLower()));
            }
            if (!string.IsNullOrEmpty(userAdminFilterDTO.Authorities))
            {
                userListQuery = userListQuery.Where(x => x.UserRoles.Any(x => x.Role.Name.Equals(userAdminFilterDTO.Authorities)));
            }
            if (!userAdminFilterDTO.IsActive != null)
            {
                userListQuery = userListQuery.Where(x => x.IsActive == userAdminFilterDTO.IsActive);
            }
            int totalCount = await userListQuery.CountAsync();
            var pagedResult = await userListQuery
                .Skip((page - 1) * size)
                .Take(size)
                .Include(it => it.UserRoles)
                .ThenInclude(r => r.Role)
                .ToListAsync();

            var pagination = new PaginationDTO<FullProfileDTO>()
            {
                Data = _mapper.Map<List<FullProfileDTO>>(pagedResult),
                Total = totalCount,
                Page = page,
                Size = size
            };
            return pagination;
        }

        public async Task<OperationResult<List<(string Email, string Password)>>> ImportUsersFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return OperationResult<List<(string Email, string Password)>>.FailureResult(new[] { "File is empty or null." });
            }

            var errorDetails = new List<string>();
            var newUsers = new List<(string Email, string Password)>();

            try
            {
                _unitOfWork.BeginTransaction();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            return OperationResult<List<(string Email, string Password)>>.FailureResult(new[] { "No worksheet found in the Excel file." });
                        }

                        var columnNames = GetColumnMappings(worksheet);
                        if (!ValidateRequiredColumns(columnNames, out var missingColumnsError))
                        {
                            return OperationResult<List<(string Email, string Password)>>.FailureResult(new[] { missingColumnsError });
                        }

                        for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                        {
                            if (!ValidateRowFields(worksheet, columnNames, row, out string errorMessage))
                            {
                                errorDetails.Add(errorMessage);
                                continue;
                            }

                            var (email, password, userCreationErrors) = await CreateUserAsync(worksheet, columnNames, row);
                            if (!string.IsNullOrEmpty(userCreationErrors))
                            {
                                errorDetails.Add($"Row {row}: {userCreationErrors}");
                                continue;
                            }
                            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                            {
                                newUsers.Add((email, password));
                            }
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                //await SendAccountCreationEmailsAsync(newUsers);

                await _unitOfWork.CommitAsync();

                return errorDetails.Any()
                    ? OperationResult<List<(string Email, string Password)>>.FailureResult(errorDetails)
                    : OperationResult<List<(string Email, string Password)>>.SuccessResult(newUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during user import: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                errorDetails.Add($"Unexpected error: {ex.Message}");
                return OperationResult<List<(string Email, string Password)>>.FailureResult(errorDetails);
            }
        }

        private Dictionary<string, int> GetColumnMappings(ExcelWorksheet worksheet)
        {
            var columnNames = new Dictionary<string, int>();
            for (int col = 1; col <= worksheet.Dimension.Columns; col++)
            {
                var columnName = worksheet.Cells[1, col].Text;
                if (!string.IsNullOrEmpty(columnName))
                {
                    columnNames[columnName] = col;
                }
            }
            return columnNames;
        }

        private bool ValidateRequiredColumns(Dictionary<string, int> columnNames, out string error)
        {
            var requiredColumns = new[] { ExcelSheetConstant.Email, ExcelSheetConstant.FullName };
            var missingColumns = requiredColumns.Where(column => !columnNames.ContainsKey(column)).ToList();

            if (missingColumns.Any())
            {
                error = $"Missing required columns: {string.Join(", ", missingColumns)}.";
                return false;
            }

            error = null;
            return true;
        }

        private async Task<(string Email, string Password, string Error)> CreateUserAsync(ExcelWorksheet worksheet, Dictionary<string, int> columnNames, int row)
        {
            var email = worksheet.Cells[row, columnNames[ExcelSheetConstant.Email]].Text;
            var fullName = worksheet.Cells[row, columnNames[ExcelSheetConstant.FullName]].Text;
            var password = GenerateRandomPassword(8);
            var studentCode = worksheet.Cells[row, columnNames[ExcelSheetConstant.StudentCode]].Text;
            var phoneNumber = worksheet.Cells[row, columnNames[ExcelSheetConstant.PhoneNumber]].Text;
            var idCardNumber = worksheet.Cells[row, columnNames[ExcelSheetConstant.IdCardNumber]].Text;
            var address = worksheet.Cells[row, columnNames[ExcelSheetConstant.Address]].Text;
            var academicYear = worksheet.Cells[row, columnNames[ExcelSheetConstant.AcademicYear]].Text;

            if (await _userManager.FindByEmailAsync(email) != null)
            {
                return (null, null, null);
            }

            var newUser = new User
            {
                Email = email,
                UserName = email,
                FullName = fullName,
                EmailConfirmed = true, // Set default, adjust as needed
                ProfilePicture = ProfileConstant.defaultAvatarUrl, // Default avatar
                PhoneNumber = phoneNumber,
                StudentCode = studentCode,
                AcademicYear = academicYear,
                Address = address,
                IdCardNumber = idCardNumber,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(newUser, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (null, null, $"Failed to create user. Errors: {errors}");
            }

            await _userManager.AddToRoleAsync(newUser, RoleConstants.USER);
            return (email, password, null);
        }

        private async Task SendAccountCreationEmailsAsync(List<(string Email, string Password)> users)
        {
            foreach (var (email, password) in users)
            {
                await _emailService.SendAccountInfoMailAsync(email, password);
            }
        }




        private bool ValidateRowFields(ExcelWorksheet worksheet,Dictionary<string, int> columnNames,int row,out string errorMessage)
        {
            var requiredFields = new[]
            {
                ExcelSheetConstant.Email,
                ExcelSheetConstant.FullName,
                ExcelSheetConstant.StudentCode,
                ExcelSheetConstant.PhoneNumber,
                ExcelSheetConstant.IdCardNumber,
                ExcelSheetConstant.Address,
                ExcelSheetConstant.AcademicYear
            };

            foreach (var field in requiredFields)
            {
                if (!columnNames.ContainsKey(field))
                {
                    errorMessage = $"Missing required column: {field}.";
                    return false;
                }

                var value = worksheet.Cells[row, columnNames[field]].Text;
                if (string.IsNullOrWhiteSpace(value))
                {
                    errorMessage = $"Row {row}: Field '{field}' cannot be empty.";
                    return false;
                }
            }

            errorMessage = null;
            return true;
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

        public async Task<ICollection<UserProject>> GetProjectsByUserId(string userId)
        {
            var user = await _userManager.Users
                .Include(u => u.UserProjects)
                .ThenInclude(up => up.Project)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                throw new NotFoundException(MessageConstant.NotFoundUserError);
            }

            return user.UserProjects;
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
            var userStatus = await _projectRepository.GetUserStatusInProject(userId, projectId);

            var userProject = new UserProject
            {
                UserId = userId,
                ProjectId = projectId,
                User = user,
                Project = project,
                RoleInTeam = userRoleInProject,
                Status = userStatus,
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
            var roleInContract = await _contractRepository.GetUserRoleInContract(userId, contractId);
            var userInContract = new UserContract
            {
                UserId = userId,
                ContractId = contractId,
                User = user,
                Contract = chosenContract,
                Role = roleInContract
            };
            return userInContract;
        }
        public async Task<bool> IsUserInProject(string userId, string projectId)
        {
            return await _userRepository.CheckIfUserInProject(userId, projectId);
        }

        public async Task ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException($"Unable to deacactivate user {userId}");
            }
            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
        }
    }
}
