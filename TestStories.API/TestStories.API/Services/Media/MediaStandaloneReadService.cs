using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestStories.API.Common;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.CloudSearch.Service.Interface;
using TestStories.Common;
using TestStories.Common.Configurations;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;

namespace TestStories.API.Services
{
    public class MediaStandaloneReadService : IMediaStandaloneReadService
    {
        private const string All = "all";
        private const int UrlValidityInHours = 8760;
        private readonly TestStoriesContext _context;
        private readonly ILogger<MediaReadService> _logger;
        private readonly ICloudMediaSearchProvider _cloudMediaSearchProvider;
        private readonly IS3BucketService _s3BucketService;
        private AppSettings _appSettings { get; set; }

        public MediaStandaloneReadService(TestStoriesContext ctx, ILogger<MediaReadService> logger, ICloudMediaSearchProvider cloudMediaSearchProvider, IS3BucketService s3BucketService, IOptions<AppSettings> appSettings)
        {
            _context = ctx;
            _logger = logger;
            _cloudMediaSearchProvider = cloudMediaSearchProvider;
            _s3BucketService = s3BucketService;
            _appSettings = appSettings.Value;
        }


        public async Task<string> GetMediaDownloadUrlStandaloneAsync(int id)
        {
            string result = "";

            var media = await _context.Media.FirstOrDefaultAsync(m => m.Id == id);

            if (media == null)
            { 
                return null; 
            }
            var fileDetail = await _s3BucketService.GetFileDetail(media.Url);
            result = _s3BucketService.GeneratePreSignedURL(fileDetail, UrlValidityInHours);

            return result;
        }

        public async Task<IEnumerable<dynamic>> GetMediaStandaloneAsync(string mediaTypes, string ids, string fields)
        {
            bool allMediaTypes = mediaTypes.Equals(All, StringComparison.OrdinalIgnoreCase);
            bool allIds = ids.Equals(All, StringComparison.OrdinalIgnoreCase);
            bool allFields = fields.Equals(All, StringComparison.OrdinalIgnoreCase);


            string[] fieldNames = fields.Split(",").Select(c => c.Trim()).ToArray();
            long[] mediaIds = allIds ? new long[] { } : ids.Split(",").Select(c => long.Parse(c.Trim())).ToArray();
            int[] mediaTypeEnums = allMediaTypes ? new int[] { } : mediaTypes.Split(",").Select(c => (int)Enum.Parse(typeof(MediaTypeEnum), c.Trim())).ToArray();

            return await GetMediaAsync(allMediaTypes, allIds, allFields, mediaTypeEnums, mediaIds, fieldNames);
        }

        private async Task<IEnumerable<dynamic>> GetMediaAsync(bool allMediaTypes, bool allIds, bool allFields, int[] mediaTypeEnums, long[] ids, string[] fieldNames)
        {
            List<dynamic> result = new List<dynamic>();

            var medias = await _context.Media
                .Include(m => m.Mediatype)
                .Include(m => m.Source)
                .Include(x => x.MediaAnnotation)
                .Include(x => x.MediaSrt)
                .Include(x => x.MediaResourceOrder)
                .Include(x => x.ToolMedia)
                    .ThenInclude(x => x.Tool)
                .Include(m => m.MediaTopic)
                    .ThenInclude(mt => mt.Topic)

                .Include(m => m.Series)
                    .ThenInclude(x => x.ToolSeries)
                        .ThenInclude(x => x.Tool)
                .Where(m => m.MediastatusId == (byte)MediaStatusEnum.Published && m.IsDeleted == false)
                .Where(m => allIds || ids.Contains(m.Id))
                .Where(m => allMediaTypes || mediaTypeEnums.Contains(m.MediatypeId))
                .ToListAsync();

            foreach (var media in medias)
            {
                var toolDetail = GetToolDetails(media);

                List<ShortToolModel> tools = null;
                if (allFields ||
                    fieldNames.Contains("Tools", StringComparer.OrdinalIgnoreCase) ||
                    fieldNames.Contains("MediaTools", StringComparer.OrdinalIgnoreCase))
                {
                    tools = await GetTools(media);
                }


                var mediaResult = ComposeMediaViewModel(media, toolDetail, tools, allFields, fieldNames);

                result.Add(mediaResult);
            }
            return result;
        }

        private ShortToolModel GetToolDetails(Media media)
        {
            ShortToolModel toolDetail = null;
            if (media.ToolMedia != null && media.ToolMedia.Count > 0)
            {

                var rand = new Random();
                var toolMedia = media.ToolMedia.ElementAt(rand.Next(media.ToolMedia.Count));
                toolDetail = new ShortToolModel()
                {
                    Id = toolMedia.Tool.Id,
                    ToolName = toolMedia.Tool.Name,
                    Url = toolMedia.Tool.Url,
                    Description = toolMedia.Tool.Description,
                    FeaturedImage = toolMedia.Tool.FeaturedImage,
                    ShowOnMenu = toolMedia.Tool.ShowOnMenu,
                    ShowOnHomePage = toolMedia.Tool.ShowOnHomepage
                };
            }
            else if (media.Series != null && media.Series.ToolSeries != null && media.Series.ToolSeries.Count > 0)
            {
                var rand = new Random();
                var toolSeries = media.Series.ToolSeries.ElementAt(rand.Next(media.Series.ToolSeries.Count));
                toolDetail = new ShortToolModel()
                {
                    Id = toolSeries.Tool.Id,
                    ToolName = toolSeries.Tool.Name,
                    Url = toolSeries.Tool.Url,
                    Description = toolSeries.Tool.Description,
                    FeaturedImage = toolSeries.Tool.FeaturedImage,
                    ShowOnMenu = toolSeries.Tool.ShowOnMenu,
                    ShowOnHomePage = toolSeries.Tool.ShowOnHomepage
                };
            }

            if (toolDetail != null)
            {
                var toolFeaturedImage = toolDetail.FeaturedImage;
                toolDetail.FeaturedImage = !string.IsNullOrEmpty(toolFeaturedImage) ? _s3BucketService.RetrieveImageCDNUrl(toolFeaturedImage) : string.Empty;
                toolDetail.FeaturedImages = _s3BucketService.GetThumbnailImages(toolFeaturedImage, EntityType.Tools);
            }

            return toolDetail;
        }

        private async Task<List<ShortToolModel>> GetTools(Media media)
        {
            List<ShortToolModel> tools = null;
            if (media.MediaResourceOrder != null && media.MediaResourceOrder.Count > 0 && !string.IsNullOrEmpty(media.MediaResourceOrder.FirstOrDefault().ResourceIds.Trim()))
            {
                var dbMediaResource = media.MediaResourceOrder.FirstOrDefault();
                var logMediaResource = new { dbMediaResource.Id, Mediaid = dbMediaResource.MediaId, dbMediaResource.ResourceIds };
                var jsonString = LogsandException.GetCurrentInputJsonString(logMediaResource);
                _logger.LogInformation($"Get:Received request for Media service and action:GetMediaAsync db data details:{jsonString}");

                var resourceIds = dbMediaResource.ResourceIds.Trim().Split(",").ToList();
                var mos = 0;
                var mediaResourceIds = resourceIds.Where(m => int.TryParse(m, out mos)).Select(m => int.Parse(m)).ToList();
                if (mediaResourceIds.Count > 0)
                {
                    var items = await _context.Tool
                        .Where(t => mediaResourceIds.Contains(t.Id))
                        .Select(x => new ShortToolModel
                        {
                            Id = x.Id,
                            ToolName = x.Name,
                            Url = x.Url,
                            Description = x.Description,
                            FeaturedImage = x.FeaturedImage,
                            ShowOnMenu = x.ShowOnMenu,
                            ShowOnHomePage = x.ShowOnHomepage
                        })
                        .ToListAsync();
                    tools = items.OrderBy(d => mediaResourceIds.IndexOf(d.Id)).ToList();
                }
            }
            else
            {
                tools = media.ToolMedia.Select(item => new ShortToolModel()
                {
                    Id = item.Tool.Id,
                    ToolName = item.Tool.Name,
                    Url = item.Tool.Url,
                    Description = item.Tool.Description,
                    FeaturedImage = item.Tool.FeaturedImage,
                    ShowOnMenu = item.Tool.ShowOnMenu,
                    ShowOnHomePage = item.Tool.ShowOnHomepage
                }).GroupBy(p => p.Id).Select(grp => grp.FirstOrDefault()).ToList();
            }
            foreach (var tool in tools)
            {
                var toolFeaturedImage = tool.FeaturedImage;
                tool.FeaturedImage = !string.IsNullOrEmpty(toolFeaturedImage) ? _s3BucketService.RetrieveImageCDNUrl(toolFeaturedImage) : string.Empty;
                tool.FeaturedImages = _s3BucketService.GetThumbnailImages(toolFeaturedImage, EntityType.Tools);
            }

            return tools;
        }

        private dynamic ComposeMediaViewModel(Media media, ShortToolModel toolDetail, List<ShortToolModel> tools, bool allFields, string[] fieldNames)
        {
            dynamic mediaViewModel = new System.Dynamic.ExpandoObject();

            if (allFields || fieldNames.Contains("Id", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.id = media.Id;
            }

            if (allFields || fieldNames.Contains("Name", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.name = media.Name;
            }
            
            if (allFields || fieldNames.Contains("Description", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.description = media.Description;
            }

            if (allFields || fieldNames.Contains("longDescription", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.longDescription = media.LongDescription;
            }
            
            if (allFields || fieldNames.Contains("Url", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.url = _appSettings.IsHlsFormatEnabled == true && media.MediatypeId == 1 ? media.HlsUrl : media.Url;
            }

            if (allFields || fieldNames.Contains("EmbeddedCode", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.embeddedCode = media.EmbeddedCode;
            }
           
            if (allFields || fieldNames.Contains("logo", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.logo = media.FeaturedImage;
            }

            if (allFields || fieldNames.Contains("ImageFileName", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.imageFileName = media.FeaturedImageMetadata;
            }

            if (allFields || fieldNames.Contains("Thumbnail", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.thumbnail = media.Thumbnail;
            }

            if (allFields || fieldNames.Contains("FeaturedImage", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.featuredImage = media.FeaturedImage;
            }
            
            if (allFields || fieldNames.Contains("MediaType", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.mediaType = media.Mediatype?.Name;
            }
            
            //MediaStatus = media.Mediastatus?.Name,
            if (allFields || fieldNames.Contains("PublishDate", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.publishDate = media.DatePublishedUtc;
            }
            
            if (allFields || fieldNames.Contains("CreatedDate", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.createdDate = media.DateCreatedUtc;
            }
           
            if (allFields || fieldNames.Contains("series", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.series = media.Series?.Name;
            }

            if (allFields || fieldNames.Contains("SeriesId", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.seriesId = media.Series?.Id ?? 0;
            }

            if (allFields || fieldNames.Contains("Topic", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.topic = media.MediaTopic?.Select(x => x.Topic.Name).ToList();
            }

            if (allFields || fieldNames.Contains("TopicIds", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.topicIds = media.MediaTopic?.Select(x => x.TopicId).ToList();
            }

            if (allFields || fieldNames.Contains("Source", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.source = media.Source?.Name;
            }

            if (allFields || fieldNames.Contains("SourceId", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.sourceId = media.Source?.Id ?? 0;
            }
            
            //Tags = media.MediaTag?.Select(x => x.Tag.Name).ToList(),
            if (allFields || fieldNames.Contains("MediaAnnotations", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.mediaAnnotations = media.MediaAnnotation?.Select(ma => new MediaAnnotationModel
                {
                    TimeStamp = ma.StartAt.ToString(),
                    Duration = ma.Duration,
                    Text = ma.Title,
                    TypeId = ma.TypeId,
                    ResourceId = ma.ResourceId,
                    Link = ma.Link
                }).ToList();
            }

            if (allFields || fieldNames.Contains("IsPrivate", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.isPrivate = media.IsPrivate;
            }

            if (allFields || fieldNames.Contains("IsSharingAllowed", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.isSharingAllowed = (bool)media.IsSharingAllowed;
            }

            if (allFields || fieldNames.Contains("ActiveFromUtc", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.activeFromUtc = media.ActiveFromUtc;
            }

            if (allFields || fieldNames.Contains("ActiveToUtc", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.activeToUtc = media.ActiveToUtc;
            }

            if (allFields || fieldNames.Contains("MediaTypeId", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.mediaTypeId = media.MediatypeId;
            }

            if (allFields || fieldNames.Contains("MediaMetaData", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.mediaMetaData = media.Metadata;
            }
            
            if (allFields || fieldNames.Contains("PublishedById", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.publishedById = media.PublishUserId;
            }

            if (allFields || fieldNames.Contains("SrtFile", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.srtFile = media.SrtFile;
            }

            if (allFields || fieldNames.Contains("SrtFileName", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.srtFileName = media.SrtFileMetadata;
            }
            
            if (allFields || fieldNames.Contains("LstSrtFile", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.lstSrtFile = media.MediaSrt?.Select(msrt => new SrtFileModel
                {
                    SrtFileName = msrt.FileMetadata,
                    Uuid = msrt.File,
                    SrtLanguage = msrt.Language
                }).ToList();

                if (mediaViewModel.lstSrtFile != null && mediaViewModel.lstSrtFile.Count > 0)
                {
                    foreach (var item in mediaViewModel.lstSrtFile)
                    {
                        item.SrtFile = "";
                        item.PreSignedUrl = item != null ? _s3BucketService.RetrieveImageCDNUrl(item.Uuid) : string.Empty;
                    }
                }
                else
                {
                    mediaViewModel.lstSrtFile = null;
                }
            }

            if (allFields || fieldNames.Contains("SeoUrl", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.seoUrl = media.SeoUrl;
            }

            if (allFields || fieldNames.Contains("DraftMediaSeoUrl", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.draftMediaSeoUrl = media.DraftMediaSeoUrl;
            }

            if (allFields || fieldNames.Contains("LastUpdatedDate", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.lastUpdatedDate = media.DateLastupdatedUtc;
            }

            if (allFields || fieldNames.Contains("IsVisibleOnGoogle", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.isVisibleOnGoogle = media.IsVisibleOnGoogle;
            }

            if (allFields || fieldNames.Contains("UniqueId", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.uniqueId = media.UniqueId;
            }

            if (allFields || fieldNames.Contains("Tool", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.tool = toolDetail;
            }

            if (allFields || fieldNames.Contains("Tools", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.tools = tools;
            }

            if (allFields || fieldNames.Contains("MediaTools", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.mediaTools = tools?.Select(t => t.ToolName).ToList();
            }

            var logo = media.FeaturedImage;

            if (allFields || fieldNames.Contains("Logo", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.Logo = !string.IsNullOrEmpty(logo) ? _s3BucketService.RetrieveImageCDNUrl(logo) : string.Empty;
            }

            if (allFields || fieldNames.Contains("Logos", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.Logos = _s3BucketService.GetThumbnailImages(logo, EntityType.Media);
            }

            var thumbnail = media.Thumbnail;
            if (allFields || fieldNames.Contains("Thumbnail", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.thumbnail = !string.IsNullOrEmpty(thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(thumbnail) : string.Empty;
            }

            if (allFields || fieldNames.Contains("Thumbnails", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.thumbnails = _s3BucketService.GetThumbnailImages(thumbnail, EntityType.Media);
            }

            var featuredImage = media.FeaturedImage;

            if (allFields || fieldNames.Contains("FeaturedImage", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.featuredImage = !string.IsNullOrEmpty(featuredImage) ? _s3BucketService.RetrieveImageCDNUrl(featuredImage) : string.Empty;
            }

            if (allFields || fieldNames.Contains("FeaturedImages", StringComparer.OrdinalIgnoreCase))
            {
                mediaViewModel.featuredImages = _s3BucketService.GetThumbnailImages(featuredImage, EntityType.Media);
            }

            return mediaViewModel;
        }
    }
}
