using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;
using TestStories.Common.Configurations;
using Tag = Amazon.S3.Model.Tag;

namespace TestStories.API.Services
{
    public class S3BucketService : IS3BucketService
    {
        static readonly RegionEndpoint Endpoint = RegionEndpoint.GetBySystemName(EnvironmentVariables.AwsRegion);
        readonly AppSettings _appSettings;
        readonly ImageSettings _imageSettings;

        public S3BucketService(IOptions<AppSettings> appSettings, IOptions<ImageSettings> imageSettings)
        {
            _appSettings = appSettings.Value;
            _imageSettings = imageSettings.Value;
        }

        public static async Task<string> Sign(GetPreSignedUrlRequest parameters)
        {
            var s3Client = new AmazonS3Client(Endpoint);
            return s3Client.GetPreSignedURL(parameters);
        }

        public async Task<string> UploadXmlFileAsync(string data, string fileName)
        {
            using var client = new AmazonS3Client(Endpoint);
            var bytes = Encoding.UTF8.GetBytes(data);
            var request = new PutObjectRequest
            {
                InputStream = new MemoryStream(bytes, 0, bytes.Length),
                BucketName = EnvironmentVariables.S3BucketMedia,
                Key = fileName,
                ContentType = "text/xml",
                CannedACL = S3CannedACL.Private,
                TagSet = new List<Tag>()
            };

            request.Headers.CacheControl = _appSettings.CacheControlDuration;
            var response = await client.PutObjectAsync(request);
            return response.HttpStatusCode.ToString() == "OK" ? request.Key : string.Empty;
        }

        public async Task<string> UploadExcelFileAsync(MemoryStream data, string fileName)
        {
            using var client = new AmazonS3Client(Endpoint);
            var request = new PutObjectRequest
            {
                InputStream = data,
                BucketName = EnvironmentVariables.S3BucketMedia,
                Key = $"{fileName}.xls",
                ContentType = "application/vnd.ms-excel",
                CannedACL = S3CannedACL.Private,
                TagSet = new List<Tag>()
            };

            request.Headers.CacheControl = _appSettings.CacheControlDuration;
            var response = await client.PutObjectAsync(request);
            return response.HttpStatusCode.ToString() == "OK" ? request.Key : string.Empty;
        }
        public async Task<string> UploadImageFromBase64Async(string base64String, long entityId, EntityType entityType, string fileType)
        {
            var imageBytes = Convert.FromBase64String(base64String);
            var stream = new MemoryStream(imageBytes, 0, imageBytes.Length);
            using var client = new AmazonS3Client(Endpoint);
            var request = new PutObjectRequest
            {
                InputStream = stream,
                BucketName = EnvironmentVariables.S3BucketMedia,
                Key = $"{Guid.NewGuid()}.jpeg",
                ContentType = "image/jpeg",
                CannedACL = S3CannedACL.Private,
                TagSet = new List<Tag>()
            };

            return await SendRequestAsync(request, entityId, entityType, fileType);
        }

        public async Task<string> UploadFileToStorageAsync(IFormFile file)
        {
            using var client = new AmazonS3Client(Endpoint);
            var request = new PutObjectRequest
            {
                InputStream = file.OpenReadStream(),
                BucketName = EnvironmentVariables.S3BucketMedia,
                Key = $"{Guid.NewGuid()}" + Path.GetExtension(file.FileName),
                ContentType = file.ContentType,
                CannedACL = S3CannedACL.Private,
                TagSet = new List<Tag>()
            };

            request.Headers.CacheControl = _appSettings.CacheControlDuration;
            var response = await client.PutObjectAsync(request);
            return response.HttpStatusCode.ToString() == "OK" ? request.Key : string.Empty;
        }

        public async Task<string> UploadFileByTypeToStorageAsync(IFormFile file, long entityId, EntityType entityType, string fileType)
        {
            using var client = new AmazonS3Client(Endpoint);
            var request = new PutObjectRequest
            {
                InputStream = file.OpenReadStream(),
                BucketName = EnvironmentVariables.S3BucketMedia,
                Key = $"{Guid.NewGuid()}" + Path.GetExtension(file.FileName),
                ContentType = file.ContentType,
                CannedACL = S3CannedACL.Private,
                TagSet = new List<Tag>()
            };

            return await SendRequestAsync(request, entityId, entityType, fileType);
        }

        public async Task<bool> RemoveImageAsync(string fileName)
        {
            var result = true;
            using (var client = new AmazonS3Client(Endpoint))
            {
                if (await FileExists(fileName))
                {
                    var tags = (await GetTags(fileName)).Tagging;
                    var entityType = tags.FirstOrDefault(tag => tag.Key == "EntityType");

                    var keys = new List<string>();
                    keys.Add(fileName);

                    fileName = fileName.Replace(fileName.Split(".").Last(), "jpeg");
                    if (entityType != null)
                    {
                        if (fileName.Contains(".thumb."))
                        {
                            keys.Add(_imageSettings.Media.Banner + "/" + fileName);
                            keys.Add(_imageSettings.Media.Grid + "/" + fileName);
                            keys.Add(_imageSettings.Media.Thumbnail + "/" + fileName);
                        }

                        if (entityType.Value == "Media")
                        {
                            keys.Add(_imageSettings.Media.Banner + "/" + fileName);
                            keys.Add(_imageSettings.Media.Grid + "/" + fileName);
                            keys.Add(_imageSettings.Media.Thumbnail + "/" + fileName);
                        }
                        else if (entityType.Value == "Tools")
                        {
                            keys.Add(_imageSettings.Tool.Grid + "/" + fileName);
                            keys.Add(_imageSettings.Tool.Thumbnail + "/" + fileName);
                            keys.Add(_imageSettings.Tool.SmallThumbnail + "/" + fileName);
                        }
                        else if (entityType.Value == "Series")
                        {
                            keys.Add(_imageSettings.Series.Banner + "/" + fileName);
                            keys.Add(_imageSettings.Series.Grid + "/" + fileName);
                            keys.Add(_imageSettings.Series.Thumbnail + "/" + fileName);
                        }
                        else if (entityType.Value == "Topics")
                        {
                            keys.Add(_imageSettings.Topic.Banner + "/" + fileName);
                            keys.Add(_imageSettings.Topic.Grid + "/" + fileName);
                        }
                    }

                    foreach (var key in keys)
                    {
                        if (await FileExists(key))
                        {
                            await client.DeleteObjectAsync(new DeleteObjectRequest { BucketName = EnvironmentVariables.S3BucketMedia, Key = key });
                        }
                    }
                }
            }

            return result;
        }

        public string RetrieveImageCDNUrl(string fileName, string size = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (size != null)
            {
                fileName = fileName.Replace(fileName.Split(".").Last(), "jpeg");
                return !string.IsNullOrEmpty(_imageSettings.CDN_Domain) ? $"https://{_imageSettings.CDN_Domain}/{size}/{fileName}" : $"https://{EnvironmentVariables.S3BucketMedia}.s3.{EnvironmentVariables.AwsRegion}.amazonaws.com/{size}/{fileName}";
            }

            return !string.IsNullOrEmpty(_imageSettings.CDN_Domain) ? $"https://{_imageSettings.CDN_Domain}/{fileName}" : $"https://{EnvironmentVariables.S3BucketMedia}.s3.{EnvironmentVariables.AwsRegion}.amazonaws.com/{fileName}";
        }

        public async Task<GetObjectTaggingResponse> GetTags(string keyName)
        {
            using var s3Client = new AmazonS3Client(Endpoint);
            return await s3Client.GetObjectTaggingAsync(new GetObjectTaggingRequest
            {
                BucketName = EnvironmentVariables.S3BucketMedia,
                Key = keyName
            });
        }

        public async Task<bool> FileExists(string keyName)
        {
            using var s3Client = new AmazonS3Client(Endpoint);
            var request = new ListObjectsRequest
            {
                BucketName = EnvironmentVariables.S3BucketMedia,
                Prefix = keyName,
                MaxKeys = 1
            };

            var response = await s3Client.ListObjectsAsync(request);
            return response.S3Objects.Any();
        }

        public async Task<S3Object> GetFileDetail (string keyName)
        {
            using var s3Client = new AmazonS3Client(Endpoint);
            var mediaType = keyName.Split('.').Last();
            string[] videoType = { "mov", "mp4", "m4v", "webm", "ogv", "mpg", "mpeg" };
            var bucketName = videoType.Contains(mediaType) ? EnvironmentVariables.S3BucketVideoOut : EnvironmentVariables.S3BucketVideoIn;

            var request = new ListObjectsRequest
            {
                BucketName = bucketName,
                Prefix = keyName,
                MaxKeys = 1
            };

            var response = await s3Client.ListObjectsAsync(request);
            return response.S3Objects.FirstOrDefault();
        }

        public async Task UpdateTags(string keyName, List<Tag> tags)
        {
            using var s3Client = new AmazonS3Client(Endpoint);
            using var original = await s3Client.GetObjectAsync(EnvironmentVariables.S3BucketMedia, keyName);
            var request = new PutObjectRequest
            {
                BucketName = EnvironmentVariables.S3BucketMedia,
                InputStream = original.ResponseStream,
                Key = keyName,
                TagSet = tags.Distinct().ToList()
            };

            foreach (var key in original.Headers.Keys)
            {
                request.Headers[key] = original.Headers[key];
            }

            await s3Client.PutObjectAsync(request);
        }

        public async Task<Images> GetCompressedImages(string image, EntityType entityType)
        {
            var bannerSize = string.Empty;
            var gridSize = string.Empty;
            var thumbnailSize = string.Empty;
            var smallThumbnailSize = string.Empty;
            switch (entityType)
            {
                case EntityType.Media:
                    bannerSize = _imageSettings.Media.Banner;
                    gridSize = _imageSettings.Media.Grid;
                    thumbnailSize = _imageSettings.Media.Thumbnail;
                    smallThumbnailSize = _imageSettings.Media.SmallThumbnail;
                    break;
                case EntityType.Tools:
                    bannerSize = _imageSettings.Tool.Banner;
                    gridSize = _imageSettings.Tool.Grid;
                    thumbnailSize = _imageSettings.Tool.Thumbnail;
                    smallThumbnailSize = _imageSettings.Tool.SmallThumbnail;
                    break;
                case EntityType.Series:
                    bannerSize = _imageSettings.Series.Banner;
                    gridSize = _imageSettings.Series.Grid;
                    thumbnailSize = _imageSettings.Series.Thumbnail;
                    smallThumbnailSize = _imageSettings.Series.SmallThumbnail;
                    break;
                case EntityType.Topics:
                    bannerSize = _imageSettings.Topic.Banner;
                    gridSize = _imageSettings.Topic.Grid;
                    thumbnailSize = _imageSettings.Topic.Thumbnail;
                    smallThumbnailSize = _imageSettings.Topic.SmallThumbnail;
                    break;
            }

            return !string.IsNullOrEmpty(image) ? new Images
            {
                Banner = !string.IsNullOrEmpty(bannerSize) ? RetrieveImageCDNUrl(image, bannerSize) : string.Empty,
                Grid = !string.IsNullOrEmpty(gridSize) ? RetrieveImageCDNUrl(image, gridSize) : string.Empty,
                Thumbnail = !string.IsNullOrEmpty(thumbnailSize) ? RetrieveImageCDNUrl(image, thumbnailSize) : string.Empty,
                SmallThumbnail = !string.IsNullOrEmpty(smallThumbnailSize) ? RetrieveImageCDNUrl(image, smallThumbnailSize) : string.Empty
            } : new Images();
        }

        public Images GetThumbnailImages(string image, EntityType entityType)
        {
            var bannerSize = string.Empty;
            var gridSize = string.Empty;
            var thumbnailSize = string.Empty;
            var smallThumbnailSize = string.Empty;
            switch (entityType)
            {
                case EntityType.Media:
                    bannerSize = _imageSettings.Media.Banner;
                    gridSize = _imageSettings.Media.Grid;
                    thumbnailSize = _imageSettings.Media.Thumbnail;
                    smallThumbnailSize = _imageSettings.Media.SmallThumbnail;
                    break;
                case EntityType.Tools:
                    bannerSize = _imageSettings.Tool.Banner;
                    gridSize = _imageSettings.Tool.Grid;
                    thumbnailSize = _imageSettings.Tool.Thumbnail;
                    smallThumbnailSize = _imageSettings.Tool.SmallThumbnail;
                    break;
                case EntityType.Series:
                    bannerSize = _imageSettings.Series.Banner;
                    gridSize = _imageSettings.Series.Grid;
                    thumbnailSize = _imageSettings.Series.Thumbnail;
                    smallThumbnailSize = _imageSettings.Series.SmallThumbnail;
                    break;
                case EntityType.Topics:
                    bannerSize = _imageSettings.Topic.Banner;
                    gridSize = _imageSettings.Topic.Grid;
                    thumbnailSize = _imageSettings.Topic.Thumbnail;
                    smallThumbnailSize = _imageSettings.Topic.SmallThumbnail;
                    break;
            }

            return !string.IsNullOrEmpty(image) ? new Images
            {
                Banner = !string.IsNullOrEmpty(bannerSize) ? RetrieveImageCDNUrl(image, bannerSize) : string.Empty,
                Grid = !string.IsNullOrEmpty(gridSize) ? RetrieveImageCDNUrl(image, gridSize) : string.Empty,
                Thumbnail = !string.IsNullOrEmpty(thumbnailSize) ? RetrieveImageCDNUrl(image, thumbnailSize) : string.Empty,
                SmallThumbnail = !string.IsNullOrEmpty(smallThumbnailSize) ? RetrieveImageCDNUrl(image, smallThumbnailSize) : string.Empty
            } : new Images();
        }

        public async Task<List<ShortMediaModel>> ReadFromExcel(string fileName)
        {
            var lstMedia = new List<ShortMediaModel>();
            using (var s3Client = new AmazonS3Client(Endpoint))
            {
                var request = new GetObjectRequest
                {
                    BucketName = EnvironmentVariables.S3BucketMedia,
                    Key = fileName
                };

                using var response = await s3Client.GetObjectAsync(request);
                using var responseStream = response.ResponseStream;
                using var reader = new StreamReader(responseStream);
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var values = line.Split(',').ToList();
                        if (values.Count >= 2)
                        {
                            lstMedia.Add(new ShortMediaModel { Id = Convert.ToInt64(values[0].Trim()), UniqueId = values[1].Trim() });
                        }
                    }
                }
            }
            return lstMedia;
        }

        public string GeneratePreSignedURL(S3Object fileInfo, double duration)
        {
            using (var s3Client = new AmazonS3Client(Endpoint))
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = fileInfo.BucketName,
                    Key = fileInfo.Key,
                    Expires = DateTime.UtcNow.AddHours(duration)
                };

                var urlString = s3Client.GetPreSignedURL(request);
                return urlString;
            }
        }


        #region Private Methods
        private async Task<string> SendRequestAsync(PutObjectRequest request, long entityId, EntityType entityType, string fileType)
        {
            using var client = new AmazonS3Client(Endpoint);
            if (entityType != EntityType.None)
            {
                request.TagSet.Add(new Tag { Key = "EntityType", Value = entityType.ToString() });
            }

            if (entityId > 0)
            {
                request.TagSet.Add(new Tag { Key = "EntityId", Value = entityId.ToString() });
            }

            if (!string.IsNullOrEmpty(fileType))
            {
                request.TagSet.Add(new Tag { Key = "FileType", Value = fileType });
            }

            request.Headers.CacheControl = _appSettings.CacheControlDuration;
            var response = await client.PutObjectAsync(request);
            return response.HttpStatusCode.ToString() == "OK" ? request.Key : string.Empty;
        }

        #endregion

    }
}
