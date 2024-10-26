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

        
    }
}

