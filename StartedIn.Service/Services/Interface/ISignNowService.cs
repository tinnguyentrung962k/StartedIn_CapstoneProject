using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface ISignNowService
    {
        public Task<string> UploadDocumentAsync(IFormFile filepath);
        public Task AddSignatureFieldAsync(string documentId, string signerEmail);
        public Task SendSignatureInviteAsync(string documentId, string signerEmail);
        public Task DownloadSignedDocumentAsync(string documentId, string savePath);
    }
}
