using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.Customize;


namespace StartedIn.Service.Services.Interface
{
    public interface ISignNowService
    {
        Task<string> UploadDocumentAsync(IFormFile file);
        Task AddSignatureFieldAsync(string documentId, List<EditableField> editableFields);
        Task<string> GenerateSigningLinkAsync(string documentId, List<EditableField> editableFields);
        Task DownloadSignedDocumentAsync(string documentId, string savePath);
    }
}
