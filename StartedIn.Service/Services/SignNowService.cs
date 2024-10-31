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

namespace StartedIn.Service.Services
{
    public class SignNowService : ISignNowService
    {
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private readonly SignNowSettings _signNowSettings;

        public SignNowService(IConfiguration configuration)
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
        public async Task<bool> RegisterWebhookAsync(string documentId, string callBackUrl)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                await AuthenticateAsync(); // Authenticate if token is missing
            }
            var webhookUrl = $"{_signNowSettings.ApiBaseUrl}/v2/event-subscriptions";
            var webHookRegister = new WebhookRegisterRequestDTO
            {
                EntityId = documentId,
                Action = "callback",
                Event = "user.document.freeform.signed",
                WebhookAttribute = new WebhookAttribute
                {
                    CallBack = callBackUrl,
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

