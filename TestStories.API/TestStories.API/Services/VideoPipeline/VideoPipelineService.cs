using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestStories.API.Models;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;
using TestStories.Common.Configurations;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;

namespace TestStories.API.Services
{
    public class VideoPipelineService : IVideoPipelineService
    {
        private readonly TestStoriesContext _context;
        private readonly ILogger<VideoPipelineService> _logger;
        readonly AppSettings _appSettings;
       
        public VideoPipelineService(TestStoriesContext context ,ILogger<VideoPipelineService> logger, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public async Task<SignedUrlModel> GetThumbnailUrl(string imageUuid)
        {
            throw new NotImplementedException();
        }

        public async Task<SignedUrlModel> RetrieveWithSignedUrl(string uuid)
        {
            var decodedId = System.Web.HttpUtility.UrlDecode(uuid);
            var media = _context.Media.SingleOrDefault(x => x.Url == decodedId || x.HlsUrl == decodedId);
            var result = string.Empty;

            if (media == null)
                throw new BusinessException("Video Not Found");
            var mediaType = uuid.Split('.').Last();

            if (mediaType == VideoFileTypeEnum.m3u8.ToString())
            {
                result = $"https://{ EnvironmentVariables.CDN_DNS_VIDEO_TRANSCODED }/{ decodedId }";
            }
            else
            {
                string[] videoType = { "mov", "mp4", "m4v", "webm", "ogv", "mpg", "mpeg" };
                var bucketName = videoType.Contains(mediaType) ? EnvironmentVariables.S3BucketVideoOut : EnvironmentVariables.S3BucketVideoIn;

                var signedParameters = new GetPreSignedUrlRequest
                {
                    BucketName = bucketName,
                    Key = decodedId,
                    Expires = DateTime.Now.AddMinutes(_appSettings.S3GetMediaLinkExpirationInMinutes),
                    Verb = HttpVerb.GET,
                    Protocol = Protocol.HTTPS
                };
                result = await S3BucketService.Sign(signedParameters);
            }
            if (result == null)
                throw new BusinessException("Error while signing the request");
            return new SignedUrlModel { Url = result, HttpVerb = HttpVerb.GET };
        }

        public async Task TranscodeError(VideoPipelineEventError source)
        {

            _logger.LogError($"Video Errored while transcoded: {source.Uuid}, {source.Message}");
            _logger.LogError($"Fetch Video Metadata form cache: {source.Uuid}");
            _logger.LogError($"Should do business logic related to error: {source.Uuid}");
            // Without VideoLibrary, transcoding Pipeline not gonna work.
            var record = await DynamoDBService.FindById(source.Uuid);
            if (record == null)
                throw new BusinessException("Video Not Found");
            var media = _context.Media.SingleOrDefault(x => x.Url == source.Uuid);
            if (media == null)
                throw new BusinessException("Video Not Found");
            record.Status = VideoPipelineStatusEnum.TranscodeError;
            await DynamoDBService.Save(record);
        }

        public async Task TranscodeSuccess(VideoPipelineEventSuccess source)
        {
            _logger.LogDebug("TranscodeSuccess");
            _logger.LogDebug($"Video Successfully transcoded: {source.Uuid}");
            _logger.LogDebug($"Fetch Video Metadata form cache: {source.Uuid}");
            _logger.LogDebug($"Saving video with metadata: {source.Uuid}");
            // Without VideoLibrary Pipeline transcoding not gonna work.
            var record = await DynamoDBService.FindById(source.Uuid);
            if (record == null)
                throw new BusinessException("Video Not Found");
            record.Status = VideoPipelineStatusEnum.TranscodeSuccess;
            await DynamoDBService.Save(record);
            _logger.LogDebug($"Video Thumbnail is available at: {record.Meta.FilePathThumbnail}");
            var media = _context.Media.SingleOrDefault(x => x.Url == source.Uuid);
            if (media == null)
                throw new BusinessException("Video Not Found");
        }

        public async Task<SignedUrlModel> UploadWithSignedUrl(VideoPipelineSignedUrlRequest source)
        {
            var isMediaExist = _context.Media.Any(x => x.Name == source.FileName.Trim());
            if (isMediaExist)
            {
                throw new BusinessException("Media Name already exist");
            }
            // TODO: should validate video type as well
            var thisDay = DateTime.Today;
            var filePath = thisDay.ToString("yyyy/MM/dd");
            // Every file should have its own UUID instead of using filenames directly
            var uuid = Guid.NewGuid().ToString();
            var key = $"{filePath}/{uuid}.{source.FileType.ToString().ToLower()}";
            var signedParameters = new GetPreSignedUrlRequest
            {
                BucketName = EnvironmentVariables.S3BucketVideoIn,
                Key = key,
                Expires = DateTime.Now.AddMinutes(_appSettings.S3PutMediaLinkExpirationInMinutes),
                Verb = HttpVerb.PUT,
                Protocol = Protocol.HTTPS
            };
            // Without VideoLibrary, transcoding Pipeline not gonna work. Lambda functions require state.
            var metadata = new VideoMetadata
            {
                Uuid = uuid,
                Meta = new Metadata
                {
                    FileName = source.FileName,
                    FileType = source.FileType,
                    FileSize = source.FileSize,
                    FileDescription = source.FileDescription,
                    FilePathOriginal = $"{filePath}/{uuid}.{source.FileType.ToString().ToLower()}",
                    FilePathTranscoded = $"{filePath}/{uuid}.mp4",
                    FilePathThumbnail = $"{filePath}/{uuid}.{EnvironmentVariables.VideoThumbnailPattern}",
                    UserId = "some-user-id",
                    BucketOriginal = EnvironmentVariables.S3BucketVideoIn,
                    BucketTranscoded = EnvironmentVariables.S3BucketVideoOut
                },
                Ttl = DateTime.Now.AddMinutes(_appSettings.S3PutMediaLinkExpirationInMinutes),
                Status = VideoPipelineStatusEnum.SignedUrl
            };
            _logger.LogDebug($"Add:Store video file metadata: {metadata}");
            await DynamoDBService.Save(metadata);

            var result = await S3BucketService.Sign(signedParameters);
            if (result == null)
                throw new BusinessException("Error while signing the request");
            string[] videoType = { "mov", "mp4", "m4v", "webm", "ogv", "mpg", "mpeg" };
            if (!videoType.Contains(source.FileType.ToString()))
            {
                metadata.Meta.FilePathThumbnail = string.Empty;
            }
            return new SignedUrlModel { Url = result, Uuid = key, ThumbnailUuid = metadata.Meta.FilePathThumbnail, HttpVerb = HttpVerb.PUT };
        }

        public async Task VideoUploaded(VideoPipelineEvent source)
        {
            _logger.LogDebug($"Video uploaded and ready to be transcoded: {source.Uuid}");
            _logger.LogDebug($"Fetch Video Metadata form cache: {source.Uuid}");
            _logger.LogDebug($"Saving current video pipeline state with metadata: {source.Uuid}");
            // Without VideoLibrary, transcoding Pipeline not gonna work.
            var record = await DynamoDBService.FindById(source.Uuid);
            if (record == null)
                throw new BusinessException("Video Not Found");
            var media = _context.Media.SingleOrDefault(x => x.Url == source.Uuid);
            if (media == null)
                throw new BusinessException("Video Not Found");
            record.Status = VideoPipelineStatusEnum.VideoUploaded;
            await DynamoDBService.Save(record);
        }
    }
}
