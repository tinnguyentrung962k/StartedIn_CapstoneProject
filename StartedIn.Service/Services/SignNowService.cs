using Microsoft.AspNetCore.Http;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO;
using StartedIn.CrossCutting.DTOs.RequestDTO.SignNowWebhookRequestDTO;
using Azure.Storage.Blobs;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Constants;
using DocumentFormat.OpenXml.Bibliography;
using StartedIn.Domain.Entities;

namespace StartedIn.Service.Services
{
    public class SignNowService : ISignNowService
    {
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private readonly SignNowSettings _signNowSettings;
        private readonly IAzureBlobService _azureBlobService;
        private readonly IDocumentFormatService _documentFormatService;

        public SignNowService(IConfiguration configuration, IAzureBlobService azureBlobService,IDocumentFormatService documentFormatService)
        {
            _httpClient = new HttpClient();
            _signNowSettings = new SignNowSettings
            {
                ApiBaseUrl = configuration.GetValue<string>("SignNowApiBaseUrl"),
                ClientId = configuration.GetValue<string>("SignNowClientId"),
                ClientSecret = configuration.GetValue<string>("SignNowClientSecret"),
                Username = configuration.GetValue<string>("SignNowUsername"),
                Password = configuration.GetValue<string>("SignNowPassword"),
            };
            _azureBlobService = azureBlobService;
            _documentFormatService = documentFormatService;
        }
        public async Task AuthenticateAsync()
        {
            var authUrl = $"{_signNowSettings.ApiBaseUrl}/oauth2/token";
            var authPayload = new
            {
                grant_type = "password",
                client_id = _signNowSettings.ClientId,
                client_secret = _signNowSettings.ClientSecret,
                username = _signNowSettings.Username,
                password = _signNowSettings.Password,
            };

            var content = new StringContent(JsonConvert.SerializeObject(authPayload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(authUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<SignNowTokenResponse>(responseContent);
                _accessToken = tokenResponse.AccessToken;

                // Set the access token for future requests
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }
            else
            {
                throw new Exception("Failed to authenticate with SignNow.");
            }
        }

        // Step 2: Upload Document
        public async Task<string> UploadDocumentAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.");

            var uploadUrl = $"{_signNowSettings.ApiBaseUrl}/document";

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(memoryStream), "file", file.FileName);

                var response = await _httpClient.PostAsync(uploadUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var documentResponse = JsonConvert.DeserializeObject<SignNowDocumentResponseDTO>(responseContent);
                    return documentResponse.Id; // Return the document ID
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to upload document. Status Code: {response.StatusCode}, Error: {errorContent}");
                }
            }
        }
        public async Task<string> UploadInvestmentContractToSignNowAsync(
        Contract contract, User investor, User leader, Project project,
        ShareEquity shareEquity, List<Disbursement> disbursements, decimal? buyPrice)
        {
            // Bước 1: Thay thế placeholders trong mẫu hợp đồng
            var modifiedMemoryStream = await _documentFormatService.ReplacePlaceHolderForInvestmentDocumentAsync(contract, investor, leader, project, shareEquity, disbursements, buyPrice);

            // Đặt lại vị trí của memoryStream về 0 trước khi upload
            modifiedMemoryStream.Position = 0;

            // Bước 2: Chuẩn bị MultipartFormDataContent cho việc upload
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(modifiedMemoryStream), "file", $"{Guid.NewGuid()}.docx");

            // Đường dẫn để upload lên SignNow
            var uploadUrl = $"{_signNowSettings.ApiBaseUrl}/document";

            // Bước 3: Gửi yêu cầu POST để upload lên SignNow
            var response = await _httpClient.PostAsync(uploadUrl, content);

            if (response.IsSuccessStatusCode)
            {
                // Xử lý phản hồi thành công
                var responseContent = await response.Content.ReadAsStringAsync();
                var documentResponse = JsonConvert.DeserializeObject<SignNowDocumentResponseDTO>(responseContent);
                return documentResponse.Id; // Trả về document ID
            }
            else
            {
                // Xử lý lỗi nếu upload thất bại
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to upload document. Status Code: {response.StatusCode}, Error: {errorContent}");
            }
        }

        public async Task<List<FreeFormInvitationResponseDTO>> CreateFreeFormInvite(string documentId, List<string> inviteEmails)
        {
            var inviteUrl = $"{_signNowSettings.ApiBaseUrl}/document/{documentId}/invite";
            var invitationResponses = new List<FreeFormInvitationResponseDTO>();

            foreach (var inviteEmail in inviteEmails)
            {
                var freeformInviteCreateDTO = new SignNowFreeFormInviteCreateDTO
                {
                    DocumentId = documentId,
                    From = _signNowSettings.Username,
                    To = inviteEmail,
                    Message = "Vui lòng xem và ký nhận tài liệu",
                    Subject = "Tài liệu cần ký",
                    OnComplete = "document_and_attachments"
                };

                var content = new StringContent(JsonConvert.SerializeObject(freeformInviteCreateDTO), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(inviteUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var inviteResponse = JsonConvert.DeserializeObject<FreeFormInvitationResponseDTO>(responseContent);
                    invitationResponses.Add(inviteResponse);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to send invite to {inviteEmail}: {errorContent}");
                }
            }

            return invitationResponses;
        }
        public async Task<bool> RegisterWebhookAsync(SignNowWebhookCreateDTO signNowWebhookCreateDTO)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                await AuthenticateAsync(); // Authenticate if token is missing
            }
            var webhookUrl = $"{_signNowSettings.ApiBaseUrl}/v2/event-subscriptions";
            var webHookRegister = new WebhookRegisterRequestDTO
            {
                EntityId = signNowWebhookCreateDTO.EntityId,
                Action = signNowWebhookCreateDTO.Action,
                Event = signNowWebhookCreateDTO.Event,
                WebhookAttribute = new WebhookAttribute
                {
                    CallBack = signNowWebhookCreateDTO.CallBackUrl,
                    SecretKey = _signNowSettings.ClientSecret
                }
            };
            var content = new StringContent(JsonConvert.SerializeObject(webHookRegister), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(webhookUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return true; // Successfully registered
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to register webhook. Status Code: {response.StatusCode}, Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error registering webhook: {ex.Message}");
            }
        }
    }
}

