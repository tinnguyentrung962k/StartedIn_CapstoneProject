using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services
{
    public class DisbursementService : IDisbursementService
    {
        private readonly IDisbursementRepository _disbursementRepository;
        private IUnitOfWork _unitOfWork;
        private readonly ILogger<Disbursement> _logger;
        private readonly IProjectRepository _projectRepository;
        public DisbursementService(IDisbursementRepository disbursementRepository, IUnitOfWork unitOfWork, ILogger<Disbursement> logger, IProjectRepository projectRepository)
        {
            _disbursementRepository = disbursementRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _projectRepository = projectRepository;
        }
        public async Task FinishedTheTransaction(string disbursementId, string projectId, string apiKey)
        {
           var project = await _projectRepository.GetProjectById(projectId);
           if (project == null)
           {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
           }
           var disbursement = await _disbursementRepository.GetDisbursementById(disbursementId);
           if (disbursement is null)
           {
                throw new NotFoundException(MessageConstant.DisbursementNotFound);
           }
           if (disbursement.Contract.ProjectId != project.Id)
           {
                throw new UnmatchedException(MessageConstant.DisbursementNotBelongToProject);
           }
           if (apiKey != DecryptString(project.HarshPayOsApiKey))
           {
                throw new UnauthorizedAccessException(MessageConstant.RolePermissionError);
           }
           try
           {
                _unitOfWork.BeginTransaction();
                disbursement.DisbursementStatus = CrossCutting.Enum.DisbursementStatusEnum.FINISHED;
                _disbursementRepository.Update(disbursement);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
           }
           catch (Exception ex) 
           {
                _logger.LogError($"An error occurred while updating a disbursement: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
           }

        }
        public async Task CancelPayment(string disbursementId, string projectId, string apiKey)
        {
            var project = await _projectRepository.GetProjectById(projectId);
            if (project == null)
            {
                throw new NotFoundException(MessageConstant.NotFoundProjectError);
            }
            var disbursement = await _disbursementRepository.GetDisbursementById(disbursementId);
            if (disbursement is null)
            {
                throw new NotFoundException(MessageConstant.DisbursementNotFound);
            }
            if (disbursement.Contract.ProjectId != project.Id)
            {
                throw new UnmatchedException(MessageConstant.DisbursementNotBelongToProject);
            }
            if (apiKey != DecryptString(project.HarshPayOsApiKey))
            {
                throw new UnauthorizedAccessException(MessageConstant.RolePermissionError);
            }
            try
            {
                _unitOfWork.BeginTransaction();
                _logger.LogInformation($"Payment for Disbursement {disbursementId} under Project {projectId} has been requested to be canceled.");
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing payment cancellation: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
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
