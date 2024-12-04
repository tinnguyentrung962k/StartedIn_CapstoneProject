using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using StartedIn.CrossCutting.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Service.Services.Interface
{
    public interface IAzureBlobService
    {
        Task<string> UploadAvatarOrCover(IFormFile image);
        Task<MemoryStream> DownloadDocumentToMemoryStreamAsync(string blobName);
        Task<string> UploadDocumentFromMemoryStreamAsync(MemoryStream memoryStream, string blobName);
        Task<string> UploadRecruitmentImage(IFormFile image);
        Task<IList<string>> UploadRecruitmentImages(IList<IFormFile> image);
        Task<BlobClient> GetBlobClientAsync(string blobName, BlobContainerEnum blobContainer);
        Task<string> UploadEvidenceOfDisbursement(IFormFile file);
        Task<IList<string>> UploadEvidencesOfDisbursement(IList<IFormFile> files);
        Task<string> UploadEvidenceOfTransaction(IFormFile file);
        Task<string> UploadTaskAttachment(IFormFile file);
        Task<string> UploadEvidenceOfConfirmation(IFormFile file);
        Task DeleteImageFromRecruitmentBlob(string imageUrl);
    }
}
