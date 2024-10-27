using Azure.Core;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using StartedIn.CrossCutting.DTOs.RequestDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO;
using System.Net.Http.Headers;
using System.Text;


namespace StartedIn.Service.Services.Interface
{
    public interface ISignNowService
    {
        Task AuthenticateAsync();
        Task<string> UploadDocumentAsync(IFormFile file);
        Task<List<FreeFormInvitationResponseDTO>> CreateFreeFormInvite(string documentId, List<string> inviteEmails);
    }
}
