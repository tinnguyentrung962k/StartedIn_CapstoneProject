using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Net.payOS.Types;
using Net.payOS;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Repository.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StartedIn.Service.Services.Interface;
using StartedIn.Domain.Entities;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Payment;
using System.Security.Cryptography;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.Enum;
using Microsoft.Extensions.Logging;

namespace StartedIn.Service.Services
{
    public class PayOsService : IPayOsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly string _apiDomain;
        private readonly IProjectRepository _projectRepository;
        private PayOS _payOS;
        private readonly IUserService _userService;
        private readonly IDisbursementRepository _disbursementRepository;
        private readonly ILogger<PayOsService> _logger;

        public PayOsService(IUnitOfWork unitOfWork, IConfiguration configuration, UserManager<User> userManager, IProjectRepository projectRepository, IUserService userService, IDisbursementRepository disbursementRepository, ILogger<PayOsService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _userManager = userManager;
            _apiDomain = _configuration.GetValue<string>("API_DOMAIN");
            _projectRepository = projectRepository;
            _userService = userService;
            _disbursementRepository = disbursementRepository;
            _logger = logger;
        }

        public async Task<PaymentResultResponseDTO> PaymentWithPayOs(string userId, string disbursementId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != RoleInTeam.Investor)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var project = await _projectRepository.GetProjectById(projectId);
            var user = await _userManager.FindByIdAsync(userId);

            var clientId = DecryptString(project.HarshClientIdPayOsKey);
            var apiKey = DecryptString(project.HarshPayOsApiKey);
            var checksumKey = DecryptString(project.HarshChecksumPayOsKey);

            _payOS = new PayOS(clientId, apiKey, checksumKey);
            List<ItemData> items = new List<ItemData>();
            var disbursement = await _disbursementRepository.GetDisbursementById(disbursementId);
            if (disbursement == null)
            {
                throw new NotFoundException(MessageConstant.DisbursementNotFound);
            }
            if (disbursement.DisbursementStatus == DisbursementStatusEnum.FINISHED)
            {
                throw new PaymentException(MessageConstant.DisbursementFinished);
            }
            try
            {
                string content = $"{user.FullName} - {disbursement.Title}";
                int expiredAt = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (60 * 5));
                long orderCodeLong = GenerateUniqueBookingCode();
                disbursement.OrderCode = orderCodeLong;
                _disbursementRepository.Update(disbursement);
                await _unitOfWork.SaveChangesAsync();
                PaymentData paymentData = new PaymentData(
                    orderCodeLong,
                    (int)Math.Ceiling(disbursement.Amount),
                    content,
                    items,
                    $"{_apiDomain}/api/projects/{projectId}/disbursements/{disbursementId}/cancel",
                    $"{_apiDomain}/api/projects/{projectId}/disbursements/{disbursementId}/return",
                    null,
                    user.FullName,
                    user.Email,
                    user.PhoneNumber,
                    null,
                    expiredAt
                );

                var createPaymentData = await _payOS.createPaymentLink(paymentData);
                if (createPaymentData == null || string.IsNullOrEmpty(createPaymentData.checkoutUrl))
                {
                    throw new Exception("Failed to create payment link");
                }

                PayOsCreatePaymentResult createPaymentResult = new PayOsCreatePaymentResult
                {
                    AccountNumber = createPaymentData.accountNumber,
                    Amount = createPaymentData.amount,
                    Bin = createPaymentData.bin,
                    CheckoutUrl = createPaymentData.checkoutUrl,
                    Currency = createPaymentData.currency,
                    Description = createPaymentData.description,
                    ExpiredAt = createPaymentData.expiredAt,
                    OrderCode = createPaymentData.orderCode,
                    PaymentLinkId = createPaymentData.paymentLinkId,
                    QrCode = createPaymentData.qrCode,
                    Status = createPaymentData.status,
                };

                PaymentResultResponseDTO paymentResultResponseDTO = new PaymentResultResponseDTO
                {
                    Code = "00",
                    Desc = "Success - Thành công",
                    CreatedPaymentResult = createPaymentResult,
                    Signature = paymentData.signature,
                };

                return paymentResultResponseDTO;
            }
            catch (Exception ex) {
                _logger.LogError("Error while creating payment link:", ex.Message);
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
        public long GenerateUniqueBookingCode()
        {
            var random = new Random();
            long orderCode;
            orderCode = random.Next(100000, 999999);
            while (_disbursementRepository.ExistsAsync(orderCode).Result)
            {
                orderCode = random.Next(100000, 999999);
            }
            return orderCode;
        }

        public async Task<PaymentLinkInformation> GetPaymentStatus(string userId, string disbursementId, string projectId)
        {
            var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
            var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
            if (projectRole != RoleInTeam.Investor && projectRole != RoleInTeam.Leader)
            {
                throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
            }
            var project = await _projectRepository.GetProjectById(projectId);
            if (project == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }
            var disbursement = await _disbursementRepository.GetDisbursementById(disbursementId);
            if (disbursement.Contract.ProjectId != projectId)
            {
                throw new UnmatchedException(MessageConstant.DisbursementNotBelongToProject);
            }
            var clientId = DecryptString(project.HarshClientIdPayOsKey);
            var apiKey = DecryptString(project.HarshPayOsApiKey);
            var checksumKey = DecryptString(project.HarshChecksumPayOsKey);

            _payOS = new PayOS(clientId, apiKey, checksumKey);
            
            // Retrieve payment status
            long orderCodeLong = (long)disbursement.OrderCode;
            var paymentStatus = await _payOS.getPaymentLinkInformation(orderCodeLong);
            if (paymentStatus == null)
            {
                throw new NotFoundException("Transaction not found");
            }

            return paymentStatus;
        }

        private string DecryptString(string cipherText)
        {
            var key = Encoding.UTF8.GetBytes("1234567890abcdef"); 
            var iv = Encoding.UTF8.GetBytes("abcdef0123456789"); 

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
