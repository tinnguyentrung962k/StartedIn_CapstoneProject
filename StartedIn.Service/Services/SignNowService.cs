using Microsoft.AspNetCore.Http;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO;
using Azure.Storage.Blobs;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Constants;
using DocumentFormat.OpenXml.Bibliography;
using StartedIn.Domain.Entities;
using StartedIn.CrossCutting.Exceptions;
using Microsoft.Extensions.Logging;
using Azure;
using System.Runtime.InteropServices;
using StartedIn.CrossCutting.DTOs.RequestDTO.SignNow;
using StartedIn.CrossCutting.DTOs.RequestDTO.SignNow.SignNowWebhookRequestDTO;

namespace StartedIn.Service.Services
{
    public class SignNowService : ISignNowService
    {
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private readonly SignNowSettings _signNowSettings;
        private readonly IDocumentFormatService _documentFormatService;
        private readonly ILogger<SignNowService> _logger;

        public SignNowService(IConfiguration configuration, IAzureBlobService azureBlobService,IDocumentFormatService documentFormatService, ILogger<SignNowService> logger)
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
            _documentFormatService = documentFormatService;
            _logger = logger;
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
        ShareEquity shareEquity, List<Disbursement> disbursements)
        {
            // Bước 1: Thay thế placeholders trong mẫu hợp đồng
            var modifiedMemoryStream = await _documentFormatService.ReplacePlaceHolderForInvestmentDocumentAsync(contract, investor, leader, project, shareEquity, disbursements);

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
        public async Task<string> UploadStartUpShareDistributionContractToSignNowAsync(
        Contract contract, User leader, Project project,
        List<ShareEquity> shareEquities, List<UserContract> usersInContract)
        {
            // Bước 1: Thay thế placeholders trong mẫu hợp đồng
            var modifiedMemoryStream = await _documentFormatService.ReplacePlaceHolderForStartUpShareDistributionDocumentAsync(contract, leader, project, shareEquities, usersInContract);

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

        public async Task<DocumentDownLoadResponseDTO> DownLoadDocument(string documentId)
        {
            try
            {
                var downloadSignNowUrl = $"{_signNowSettings.ApiBaseUrl}/document/{documentId}/download/link";

                // Send the POST request without a request body
                var response = await _httpClient.PostAsync(downloadSignNowUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    // Assuming the API response is JSON and contains the download link as a URL
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON content into the DocumentDownLoadResponseDTO
                    var downloadInfo = JsonConvert.DeserializeObject<DocumentDownLoadResponseDTO>(responseContent);
                    return downloadInfo;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("Document not found.");
                    return null;
                }
                else
                {
                    throw new DownloadErrorException(MessageConstant.DownLoadError);
                }
            }
            catch (DownloadErrorException ex)
            {
                Console.WriteLine($"Download error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return null; // Return null if there was an error
        }

        public async Task<SignNowDocumentFullResponseDTO> GetDocumentAllInfoAsync(string documentId)
        {
            if (string.IsNullOrEmpty(documentId))
            {
                throw new NotFoundException(MessageConstant.NotFoundDocumentError);
            }

            try
            {
                // Define the API endpoint with the document ID.
                var requestUri = $"{_signNowSettings.ApiBaseUrl}/document/{documentId}";

                // Send GET request to the API.
                HttpResponseMessage response = await _httpClient.GetAsync(requestUri);

                // Ensure success status code.
                response.EnsureSuccessStatusCode();

                // Read the response content as a string.
                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON response to DocumentResponseDto.
                var documentInfo = JsonConvert.DeserializeObject<SignNowDocumentFullResponseDTO>(responseContent);

                return documentInfo;
            }
            catch (HttpRequestException e)
            {
                // Handle network-related errors here.
                _logger.LogError($"Request error: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                // Handle other potential errors here.
                _logger.LogError($"Error: {e.Message}");
                throw;
            }
        }

        public async Task<bool> RegisterManyWebhookAsync(List<SignNowWebhookCreateDTO> signNowWebhooksCreateList)
        {
            // Ensure the access token is available; if not, authenticate.
            if (string.IsNullOrEmpty(_accessToken))
            {
                await AuthenticateAsync(); // Authenticate if token is missing
            }

            // Use a list to track the results of each registration attempt
            var registrationTasks = new List<Task<bool>>();

            foreach (var signNowWebhookCreateDTO in signNowWebhooksCreateList)
            {
                // Register each webhook asynchronously and add the task to the list
                registrationTasks.Add(RegisterWebhookAsync(signNowWebhookCreateDTO));
            }

            // Await all registration tasks
            var results = await Task.WhenAll(registrationTasks);

            // Return true if all registrations were successful
            return results.All(result => result);
        }

        public async Task<SignInviteFreeFormResponseDTO> GetDocumentFreeFormInvite(string documentId)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                await AuthenticateAsync(); // Authenticate if token is missing
            }

            // Construct the request URL for the free form invite
            var requestUrl = $"{_signNowSettings.ApiBaseUrl}/v2/documents/{documentId}/free-form-invites";
            try
            {
                // Send the GET request to retrieve the free form invite details
                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var signInviteFreeFormResponse = JsonConvert.DeserializeObject<SignInviteFreeFormResponseDTO>(jsonResponse);
                return signInviteFreeFormResponse; // Return the deserialized object
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving free form invite: {ex.Message}", ex);
            }
        }
        
    }
}

