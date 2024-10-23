using Microsoft.AspNetCore.Http;
using SignNow.Net.Interfaces;
using SignNow.Net.Model;
using SignNow.Net;
using StartedIn.Service.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using SignNow.Net.Service;

namespace StartedIn.Service.Services
{
    public class SignNowService
    {
        private readonly ISignNowContext _signNowContext;
        private readonly IDocumentService _documentService;
        private readonly OAuth2Service _authService;
        private Token _accessToken;

        public SignNowService(string apiBaseUrl, string clientId, string clientSecret, string username, string password)
        {
            var baseUri = new Uri(apiBaseUrl);
            _authService = new OAuth2Service(baseUri, clientId, clientSecret);

            // Get access token synchronously in the constructor (consider changing to async)
            var tokenResponse = GetAccessToken(username, password).Result;

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                throw new UnauthorizedAccessException("Unable to obtain access token.");
            }

            // Store the access token
            _accessToken = tokenResponse;

            // Initialize SignNow context with the access token
            _signNowContext = new SignNowContext(_accessToken);
            _documentService = _signNowContext.Documents;
        }

        private async Task<Token> GetAccessToken(string username, string password)
        {
            var scope = Scope.All; // Set the appropriate scope
            return await _authService.GetTokenAsync(username, password, scope);
        }

        // Uploads a document to SignNow and returns the document ID
        public async Task<string> UploadDocumentAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null.");
            }

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var uploadResponse = await _documentService.UploadDocumentAsync(memoryStream, file.FileName);
                return uploadResponse.Id;
            }
        }

        // Sends a signature invite to a signer
        //public async Task AddSignatureInviteAsync(string documentId, string signerEmail)
        //{
        //    var inviteRequest = new SignInviteRequest
        //    {
        //        To = signerEmail,
        //        From = "your-email@example.com",
        //        Message = "Please sign this document",
        //        Subject = "Signature Request",
        //        Role = "Signer", // Adjust based on your use case
        //        Order = 1        // Signing order
        //    };

        //    await _documentService.CreateSigningLinkAsync(documentId, inviteRequest);
        //}

        // Downloads the signed document as a PDF
        public async Task DownloadSignedDocumentAsync(string documentId, string savePath)
        {
            // Download the signed document
            var downloadResponse = await _documentService.DownloadDocumentAsync(documentId, DownloadType.PdfOriginal);

            // Check if the response is valid
            if (downloadResponse == null || downloadResponse.Document == null)
            {
                throw new Exception("Failed to download the signed document.");
            }
        }
    }
}
