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
using Microsoft.Extensions.Options;
using StartedIn.CrossCutting.Customize;
using Microsoft.Extensions.Configuration;

namespace StartedIn.Service.Services
{
    public class SignNowService : ISignNowService
    {
        private readonly SignNowSettings signNowSettings;
        private readonly ISignNowContext _signNowContext;
        private readonly IDocumentService _documentService;
        private readonly ISignInvite _signInvite;
        private readonly OAuth2Service _authService;
        private readonly IConfiguration _configuration;
        private Token _accessToken;

        public SignNowService(IConfiguration configuration)
        {
            _configuration = configuration;
            signNowSettings = new SignNowSettings() { 
                ApiBaseUrl = _configuration.GetValue<string>("SignNowApiBaseUrl"),
                ClientId = _configuration.GetValue<string>("SignNowClientId"),
                ClientSecret = _configuration.GetValue<string>("SignNowClientSecret"),
                Username = _configuration.GetValue<string>("SignNowUsername"),
                Password = _configuration.GetValue<string>("SignNowPassword"),
            };
            var baseUri = new Uri(signNowSettings.ApiBaseUrl);
            _authService = new OAuth2Service(baseUri, signNowSettings.ClientId, signNowSettings.ClientSecret);
            var tokenResponse = GetAccessToken(signNowSettings.Username, signNowSettings.Password).GetAwaiter().GetResult();
            _accessToken = tokenResponse;
            _signNowContext = new SignNowContext(_accessToken);
            _documentService = _signNowContext.Documents;
            _signInvite = _signNowContext.Invites;
        }

        // Retrieves an access token for authenticating with the SignNow API
        private async Task<Token> GetAccessToken(string username, string password)
        {
            var scope = Scope.All; // Define the necessary scope for your application
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
                return uploadResponse.Id; // Return the document ID for reference
            }
        }

        // Sends a signature invite to a signer
        public async Task<string> GenerateSigningLinkAsync(string documentId, List<EditableField> editableFields)
        {
            try
            {
                await AddSignatureFieldAsync(documentId, editableFields);
                // Log document ID for debugging purposes
                Console.WriteLine($"Generating signing link for Document ID: {documentId}");

                // Generate the signing link
                var signingLink = await _documentService.CreateSigningLinkAsync(documentId);

                if (signingLink == null || string.IsNullOrEmpty(signingLink.Url.ToString()))
                {
                    throw new Exception("Failed to generate a signing link.");
                }

                // Log and return the signing link
                Console.WriteLine($"Signing link generated successfully: {signingLink.Url}");
                return signingLink.Url.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while generating signing link: {ex.Message}", ex);
            }
        }
        

        // Downloads the signed document as a PDF
        public async Task DownloadSignedDocumentAsync(string documentId, string savePath)
        {
            try
            {
                // Log the document ID for debugging purposes
                Console.WriteLine($"Attempting to download document with ID: {documentId}");

                // Download the signed document as a PDF
                var downloadResponse = await _documentService.DownloadDocumentAsync(documentId, DownloadType.PdfOriginal);

                // Check if the response or the document stream is null
                if (downloadResponse == null || downloadResponse.Document == null)
                {
                    throw new Exception("Failed to download the signed document. The document stream is null.");
                }

                // Ensure the directory exists
                var directory = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Log the download progress
                Console.WriteLine($"Saving document to path: {savePath}");

                // Save the document to the specified path
                using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await downloadResponse.Document.CopyToAsync(fileStream);
                }

                // Log the success message
                Console.WriteLine($"Document downloaded and saved successfully at: {savePath}");
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException("Access denied to the specified file path.", ex);
            }
            catch (IOException ex)
            {
                throw new IOException("An I/O error occurred while saving the file.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while downloading or saving the document: {ex.Message}", ex);
            }
        }

        public async Task AddSignatureFieldAsync(string documentId, List<EditableField> editableFields)
        {
            try
            {
                // Add all the fields to a list
                var fieldsToAdd = new List<IFieldEditable>();
                foreach (var editableField in editableFields)
                {
                    fieldsToAdd.Add(editableField);
                }
                var response = await _documentService.EditDocumentAsync(documentId, fieldsToAdd);

                Console.WriteLine("Multiple fields added to document successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while adding fields: {ex.Message}");
                throw; // Rethrow or handle as appropriate
            }
        }
    }
}
