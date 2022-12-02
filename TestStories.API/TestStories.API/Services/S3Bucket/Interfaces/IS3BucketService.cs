using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;

namespace TestStories.API.Services
{
    public interface IS3BucketService
    {
        Task<string> UploadImageFromBase64Async(string base64String, long entityId, EntityType entityType, string fileType);

        Task<string> UploadFileToStorageAsync(IFormFile file);

        Task<string> UploadFileByTypeToStorageAsync(IFormFile file, long entityId, EntityType entityType, string fileType);

        string RetrieveImageCDNUrl (string fileName, string size = null);

        Task<bool> RemoveImageAsync(string fileName);

        Task<Images> GetCompressedImages (string image, EntityType entityType);

        Task<GetObjectTaggingResponse> GetTags(string keyName);

        Task UpdateTags(string keyName, List<Tag> tags);

        Task<bool> FileExists(string keyName);

        Task<S3Object> GetFileDetail (string keyName);

        Task<string> UploadXmlFileAsync(string data, string fileName);

        Task<string> UploadExcelFileAsync(MemoryStream data, string fileName);

        Task<List<ShortMediaModel>> ReadFromExcel(string fileName);

        Images GetThumbnailImages(string image, EntityType entityType);

        string GeneratePreSignedURL(S3Object objectKey, double duration);
    }
}
