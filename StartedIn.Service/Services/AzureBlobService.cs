using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.Enum;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services
{
    public class AzureBlobService : IAzureBlobService
    {
        private readonly IConfiguration _configuration;
        private readonly BlobContainerClient _pictureContainerClient;
        private readonly BlobContainerClient _postImgContainerClient;
        private readonly BlobContainerClient _documentContainerClient;
        private readonly BlobContainerClient _taskAttachmentContainerClient;
        private readonly BlobContainerClient _recruitmentImageContainerClient;
        private readonly BlobContainerClient _cvFileContainerClient;
        private readonly string _azureBlobStorageKey;

        public AzureBlobService(IConfiguration configuration)
        {
            _configuration = configuration;
            _azureBlobStorageKey = configuration.GetValue<string>("AzureBlobStorageKey");

            BlobServiceClient blobServiceClient = new BlobServiceClient(_azureBlobStorageKey);

            _pictureContainerClient = blobServiceClient.GetBlobContainerClient("avatars");
            _postImgContainerClient = blobServiceClient.GetBlobContainerClient("post-images");
            _documentContainerClient = blobServiceClient.GetBlobContainerClient("documents");
            _taskAttachmentContainerClient = blobServiceClient.GetBlobContainerClient("task-attachments");
            _recruitmentImageContainerClient = blobServiceClient.GetBlobContainerClient("recruitment-images");
            _cvFileContainerClient = blobServiceClient.GetBlobContainerClient("cv-files");
        }

        public async Task<string> UploadAvatarOrCover(IFormFile image)
        {
            if (!IsValidImageFile(image))
            {
                throw new ArgumentException("The uploaded file is not a valid image.");
            }
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var blobClient = _pictureContainerClient.GetBlobClient(fileName);

            using (var stream = image.OpenReadStream())
            using (var imageSharp = await Image.LoadAsync(stream))
            {
                imageSharp.Mutate(x => x.Resize(300, 300));
                var encoder = new JpegEncoder { Quality = 80 };
                using (var memoryStream = new MemoryStream())
                {
                    imageSharp.Save(memoryStream, encoder);
                    memoryStream.Position = 0;
                    await blobClient.UploadAsync(memoryStream);
                }
            }

                return blobClient.Uri.AbsoluteUri;
        }
        public async Task<string> UploadEvidenceOfDisbursement(IFormFile file)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var blobClient = _documentContainerClient.GetBlobClient(fileName);
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }
            return blobClient.Uri.ToString();
        }
        public async Task<string> UploadEvidenceOfConfirmation(IFormFile file)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var blobClient = _documentContainerClient.GetBlobClient(fileName);
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }
            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadEvidenceOfTransaction(IFormFile file)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var blobClient = _documentContainerClient.GetBlobClient(fileName);
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }
            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadTaskAttachment(IFormFile file)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var blobClient = _documentContainerClient.GetBlobClient(fileName);
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }
            return blobClient.Uri.ToString();
        }

        public async Task<IList<string>> UploadEvidencesOfDisbursement(IList<IFormFile> files)
        {
            var fileUrls = new List<string>();
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    fileUrls.Add(await UploadEvidenceOfDisbursement(file));
                }
            }
            return fileUrls;
        }

        public async Task<string> UploadRecruitmentImage(IFormFile image)
        {
            if (!IsValidImageFile(image))
            {
                throw new ArgumentException("The uploaded file is not a valid image.");
            }
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var blobClient = _recruitmentImageContainerClient.GetBlobClient(fileName);

            using (var stream = image.OpenReadStream())
            using (var imageSharp = await Image.LoadAsync(stream))
            {
                imageSharp.Mutate(x => x.Resize(1920, 1080));
                var encoder = new JpegEncoder { Quality = 90 };
                using (var memoryStream = new MemoryStream())
                {
                    imageSharp.Save(memoryStream, encoder);
                    memoryStream.Position = 0;
                    await blobClient.UploadAsync(memoryStream);
                }
            }
            return blobClient.Uri.AbsoluteUri;
        }

        public async Task<IList<string>> UploadRecruitmentImages(IList<IFormFile> image)
        {
            var imageUrls = new List<string>();
            if (image != null && image.Count > 0)
            {
                foreach (var img in image)
                {
                    imageUrls.Add(await UploadRecruitmentImage(img));
                }
            }
            return imageUrls;
        }

        private bool IsValidImageFile(IFormFile file)
        {
            // Get the file's content type
            var contentType = file.ContentType.ToLower();

            // Check if the content type is a valid image type
            return contentType.StartsWith("image/");
        }
        public async Task<MemoryStream> DownloadDocumentToMemoryStreamAsync(string blobName)
        {
            // Get the blob client for the specified blob in the "documents" container
            BlobClient blobClient = _documentContainerClient.GetBlobClient(blobName);

            // Initialize a new memory stream to hold the downloaded blob data
            var memoryStream = new MemoryStream();

            try
            {
                // Download the blob to the memory stream
                await blobClient.DownloadToAsync(memoryStream);

                // Reset the position of the memory stream to the beginning
                memoryStream.Position = 0;

                return memoryStream;
            }
            catch (RequestFailedException ex)
            {
                // Handle the exception as necessary (e.g., log the error, return null, rethrow, etc.)
                Console.WriteLine($"Error downloading document blob: {ex.Message}");
                memoryStream.Dispose();
                throw;
            }
        }

        public async Task<string> UploadDocumentFromMemoryStreamAsync(MemoryStream memoryStream, string blobName)
        {
            if (memoryStream == null || memoryStream.Length == 0)
            {
                throw new ArgumentException("The provided memory stream is empty.");
            }

            // Reset the memory stream position to ensure it reads from the start
            memoryStream.Position = 0;

            // Get a blob client for the documents container
            var blobClient = _documentContainerClient.GetBlobClient(blobName);

            // Upload the memory stream to the blob
            await blobClient.UploadAsync(memoryStream, overwrite: true);

            // Return the URL of the uploaded blob
            return blobClient.Uri.AbsoluteUri;
        }
        public async Task<BlobClient> GetBlobClientAsync(string blobName, BlobContainerEnum blobContainer)
        {
            if (string.IsNullOrEmpty(blobName))
            {
                throw new ArgumentException("Blob name cannot be null or empty.", nameof(blobName));
            }

            BlobContainerClient containerClient;

            // Determine the appropriate container client based on the enum
            switch (blobContainer)
            {
                case BlobContainerEnum.Avatars:
                    containerClient = _pictureContainerClient;
                    break;
                case BlobContainerEnum.PostImgs:
                    containerClient = _postImgContainerClient;
                    break;
                case BlobContainerEnum.Documents:
                    containerClient = _documentContainerClient;
                    break;
                case BlobContainerEnum.TaskAttachments:
                    containerClient = _taskAttachmentContainerClient;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(blobContainer), "Invalid blob container specified.");
            }
            var blobClient = containerClient.GetBlobClient(blobName);
            if (blobClient == null)
            {
                throw new ArgumentException(nameof(blobContainer), "Invalid blob client.");
            }
            return blobClient;
        }
        
        public async Task DeleteImageFromRecruitmentBlob(string imageUrl)
        {
            var blobName = GetBlobNameFromUrl(imageUrl);
            var blobClient = _recruitmentImageContainerClient.GetBlobClient(blobName);
            await blobClient.DeleteAsync();
        }

        public async Task<string> UploadCVFileApplication(IFormFile file)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var blobClient = _cvFileContainerClient.GetBlobClient(fileName);
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }
            return blobClient.Uri.ToString();
        }

        // Helper method to extract blob name from URL
        private string GetBlobNameFromUrl(string url)
        {
            var uri = new Uri(url);
            return uri.Segments[^1]; 
        }
    }
}
