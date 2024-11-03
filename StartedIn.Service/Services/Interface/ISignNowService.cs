using Azure.Core;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.RequestDTO.SignNowWebhookRequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO;
using StartedIn.Domain.Entities;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;


namespace StartedIn.Service.Services.Interface
{
    public interface ISignNowService
    {
        Task AuthenticateAsync();
        Task<string> UploadDocumentAsync(IFormFile file);
        Task<List<FreeFormInvitationResponseDTO>> CreateFreeFormInvite(string documentId, List<string> inviteEmails);
        Task<bool> RegisterWebhookAsync(SignNowWebhookCreateDTO signNowWebhookCreateDTO);
        Task<bool> RegisterManyWebhookAsync(List<SignNowWebhookCreateDTO> signNowWebhooksCreateList);
        Task<string> UploadInvestmentContractToSignNowAsync(Contract contract, User investor, User leader, Project project,ShareEquity shareEquity, List<Disbursement> disbursements, decimal? buyPrice);
        Task DownLoadDocument(string documentId);
        Task<DocumentResponseDTO> GetDocumentAllInfoAsync(string documentId);
        Task<SignInviteFreeFormResponseDTO> GetDocumentFreeFormInvite(string documentId);
    }
}
