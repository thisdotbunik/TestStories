using System;
using System.Collections.Generic;
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
using Newtonsoft.Json;

namespace TestStories.API.Services
{
    public class MediaReadService : IMediaReadService
    {
        private readonly TestStoriesContext _context;
        private readonly ILogger<MediaReadService> _logger;
        private readonly ICloudMediaSearchProvider _cloudMediaSearchProvider;
        private readonly IS3BucketService _s3BucketService;
        private AppSettings _appSettings { get; set; }

        public MediaReadService(TestStoriesContext ctx, ILogger<MediaReadService> logger, ICloudMediaSearchProvider cloudMediaSearchProvider, IS3BucketService s3BucketService, IOptions<AppSettings> appSettings)
        {
            _context = ctx;
            _logger = logger;
            _cloudMediaSearchProvider = cloudMediaSearchProvider;
            _s3BucketService = s3BucketService;
            _appSettings = appSettings.Value;
        }

        public async Task<GridResponse<MediaShortModel>> FilteredMedia(FilterMediaSearchRequest model)
        {
            var medias = new List<MediaShortModel>();
            if (model.mediaStatusIds != null && model.mediaTypeIds != null)
            {
                medias = _context.Media.Where(t => model.mediaStatusIds.Contains(t.MediastatusId) && model.mediaTypeIds.Contains(t.MediatypeId)).Select(x => new MediaShortModel { Id = x.Id, Name = x.Name }).ToList();
            }
            else if (model.mediaStatusIds != null)
            {
                medias = _context.Media.Where(t => model.mediaStatusIds.Contains(t.MediastatusId)).Select(x => new MediaShortModel { Id = x.Id, Name = x.Name }).ToList();
            }

            else if (model.mediaTypeIds != null)
            {
                medias = _context.Media.Where(t => model.mediaTypeIds.Contains(t.MediatypeId)).Select(x => new MediaShortModel { Id = x.Id, Name = x.Name }).ToList();
            }
            else
            {
                medias = _context.Media.Select(x => new MediaShortModel { Id = x.Id, Name = x.Name }).ToList();
            }

            var items = medias.OrderBy(x => x.Name).ToList();
            return new GridResponse<MediaShortModel> { items = items, TotalRowsAvailable = items.Count };
        }

        public async Task<MediaViewModel> GetMediaByIdAsync(int id)
        {
            var media = await _context.Media
                .Include(m => m.Mediatype)
                //.Include(m => m.Mediastatus)
                .Include(m => m.Source)
                .Include(x => x.MediaAnnotation)
                .Include(x => x.MediaSrt)
                .Include(x => x.MediaResourceOrder)
                .Include(x => x.ToolMedia)
                    .ThenInclude(x => x.Tool)
                .Include(m => m.MediaTopic)
                    .ThenInclude(mt => mt.Topic)
                //.Include(m => m.MediaTag)
                    //.ThenInclude(mt => mt.Tag)
                .Include(m => m.Series)
                    .ThenInclude(x => x.ToolSeries)
                        .ThenInclude(x => x.Tool)
                .Where(m => m.Id == id && m.MediastatusId == (byte)MediaStatusEnum.Published && m.IsDeleted == false)
                .FirstOrDefaultAsync();

            if (media == null) throw new UnauthorizedAccessException("Media is not available");

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

            var mediaViewModel = new MediaViewModel() {
                Id = media.Id,
                Name = media.Name,
                Description = media.Description,
                LongDescription = media.LongDescription,
                Url = _appSettings.IsHlsFormatEnabled == true && media.MediatypeId == 1 ? media.HlsUrl : media.Url,
                EmbeddedCode = media.EmbeddedCode,
                Logo = media.FeaturedImage,
                ImageFileName = media.FeaturedImageMetadata,
                Thumbnail = media.Thumbnail,
                FeaturedImage = media.FeaturedImage,
                MediaType = media.Mediatype?.Name,
                //MediaStatus = media.Mediastatus?.Name,
                PublishDate = media.DatePublishedUtc,
                CreatedDate = media.DateCreatedUtc,
                Series = media.Series?.Name,
                SeriesId = media.Series?.Id ?? 0,
                Topic = media.MediaTopic?.Select(x => x.Topic.Name).ToList(),
                TopicIds = media.MediaTopic?.Select(x => x.TopicId).ToList(),
                Source = media.Source?.Name,
                SourceId = media.Source?.Id ?? 0,
                //Tags = media.MediaTag?.Select(x => x.Tag.Name).ToList(),
                MediaAnnotations = media.MediaAnnotation?.Select(ma => new MediaAnnotationModel
                {
                    TimeStamp = ma.StartAt.ToString(),
                    Duration = ma.Duration,
                    Text = ma.Title,
                    TypeId = ma.TypeId,
                    ResourceId = ma.ResourceId,
                    Link = ma.Link
                }).ToList(),
                IsPrivate = media.IsPrivate,
                IsSharingAllowed = (bool)media.IsSharingAllowed,
                ActiveFromUtc = media.ActiveFromUtc,
                ActiveToUtc = media.ActiveToUtc,
                MediaTypeId = media.MediatypeId,
                MediaMetaData = media.Metadata,
                PublishedById = media.PublishUserId,
                SrtFile = media.SrtFile,
                SrtFileName = media.SrtFileMetadata,
                LstSrtFile = media.MediaSrt?.Select(msrt => new SrtFileModel
                {
                    SrtFileName = msrt.FileMetadata,
                    Uuid = msrt.File,
                    SrtLanguage = msrt.Language
                }).ToList(),
                SeoUrl = media.SeoUrl,
                DraftMediaSeoUrl = media.DraftMediaSeoUrl,
                LastUpdatedDate = media.DateLastupdatedUtc,
                IsVisibleOnGoogle = media.IsVisibleOnGoogle,
                UniqueId = media.UniqueId,
                Tool = toolDetail,
                Tools = tools,
                MediaTools = tools?.Select(t => t.ToolName).ToList()
                
            };

            var logo = mediaViewModel.Logo;
            mediaViewModel.Logo = !string.IsNullOrEmpty(logo) ? _s3BucketService.RetrieveImageCDNUrl(logo) : string.Empty;
            mediaViewModel.Logos = _s3BucketService.GetThumbnailImages(logo, EntityType.Media);

            var thumbnail = mediaViewModel.Thumbnail;
            mediaViewModel.Thumbnail = !string.IsNullOrEmpty(thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(thumbnail) : string.Empty;
            mediaViewModel.Thumbnails = _s3BucketService.GetThumbnailImages(thumbnail, EntityType.Media);

            var featuredImage = mediaViewModel.FeaturedImage;
            mediaViewModel.FeaturedImage = !string.IsNullOrEmpty(featuredImage) ? _s3BucketService.RetrieveImageCDNUrl(featuredImage) : string.Empty;
            mediaViewModel.FeaturedImages = _s3BucketService.GetThumbnailImages(featuredImage, EntityType.Media);

            if (mediaViewModel.LstSrtFile != null && mediaViewModel.LstSrtFile.Count > 0)
            {
                foreach (var item in mediaViewModel.LstSrtFile)
                {
                    item.SrtFile = "";
                    item.PreSignedUrl = item != null ? _s3BucketService.RetrieveImageCDNUrl(item.Uuid) : string.Empty;
                }
            }
            else
            {
                mediaViewModel.LstSrtFile = null;
            }

            return mediaViewModel;
        }

        public async Task<MediaViewModel> GetMediaAsync(int id, string userRole)
        {
            var dbMedia = await _context.Media
                .Include(x => x.MediaTopic)
                .ThenInclude(x => x.Topic)
                .Include(x => x.MediaTag)
                .ThenInclude(x => x.Tag)
                .Include(x => x.Mediatype)
                .Include(x => x.Mediastatus)
                .Include(x => x.MediaSrt)
                .Include(x => x.MediaResourceOrder)
                .Include(x => x.MediaAnnotation)
                .Include(x => x.ToolMedia)
                .ThenInclude(x => x.Tool)
                .Include(x => x.PublishUser)
                .Include(x => x.UploadUser)
                .Include(x => x.Source)
                .Include(x => x.Series)
                .ThenInclude(x => x.ToolSeries)
                .ThenInclude(x => x.Tool)
                .Where(x => x.Id == id).FirstOrDefaultAsync();

            // replace with embeded media
            //if(dbMedia.NewId != null && dbMedia.NewId > 0)
            //{
            //    dbMedia = await _context.Media
            //    .Include(x => x.MediaTopic)
            //    .ThenInclude(x => x.Topic)
            //    .Include(x => x.MediaTag)
            //    .ThenInclude(x => x.Tag)
            //    .Include(x => x.Mediatype)
            //    .Include(x => x.Mediastatus)
            //    .Include(x => x.MediaSrt)
            //    .Include(x => x.MediaResourceOrder)
            //    .Include(x => x.MediaAnnotation)
            //    .Include(x => x.ToolMedia)
            //    .ThenInclude(x => x.Tool)
            //    .Include(x => x.PublishUser)
            //    .Include(x => x.UploadUser)
            //    .Include(x => x.Source)
            //    .Include(x => x.Series)
            //    .ThenInclude(x => x.ToolSeries)
            //    .ThenInclude(x => x.Tool)
            //    .Where(x => x.Id == dbMedia.NewId).FirstOrDefaultAsync();
            //}

            if (dbMedia != null && !dbMedia.IsDeleted)
            {
                if ((MediaStatusEnum)dbMedia.MediastatusId != MediaStatusEnum.Published)
                {
                    if (IsMediaActive(id) && userRole != "Admin" && userRole != "Admin-Editor" && userRole != "SuperAdmin")
                    {
                        throw new UnauthorizedAccessException("You are not permitted to retrieve this entity because of user role restrictions");
                    }
                }
            }
            else
            {
                throw new UnauthorizedAccessException("Media is not available");
            }

            ShortToolModel toolDetail = null;
            List<ShortToolModel> tools = null;
            if (dbMedia.ToolMedia.Count > 0)
            {
                var rand = new Random();
                var toSkip = rand.Next(0, dbMedia.ToolMedia.Count);
                //var toolMedia = dbMedia.ToolMedia.Skip(toSkip).Take(1).First();
                if (dbMedia.ToolMedia.Skip(toSkip).Take(1).First() != null)
                {
                    //toolDetail = new ShortToolModel();
                     toolDetail = dbMedia.ToolMedia.Select(item => new ShortToolModel()
                    {
                        Id = item.Tool.Id,
                        ToolName = item.Tool.Name,
                        Url = item.Tool.Url,
                        Description = item.Tool.Description,
                        FeaturedImage = item.Tool.FeaturedImage,
                        ShowOnMenu = item.Tool.ShowOnMenu,
                        ShowOnHomePage = item.Tool.ShowOnHomepage
                    }).Skip(toSkip).Take(1).First();  
                }
            }
            else
            {
                if (dbMedia.SeriesId != null)
                {
                    //var lstToolSeries = await _context.ToolSeries.Where(x => x.SeriesId == dbMedia.SeriesId).ToListAsync();
                    if (dbMedia.Series.ToolSeries.Count > 0)
                    {
                        var rand = new Random();
                        var toSkip = rand.Next(0, dbMedia.Series.ToolSeries.Count);
                        //var toolSeries = dbMedia.Series.ToolSeries.Skip(toSkip).Take(1).First();
                        if (dbMedia.Series.ToolSeries.Skip(toSkip).Take(1).First() != null)
                        {
                            //toolDetail = new ShortToolModel();
                            toolDetail = dbMedia.Series.ToolSeries.Select(item => new ShortToolModel()
                            {
                                Id = item.Tool.Id,
                                ToolName = item.Tool.Name,
                                Url = item.Tool.Url,
                                Description = item.Tool.Description,
                                FeaturedImage = item.Tool.FeaturedImage,
                                ShowOnMenu = item.Tool.ShowOnMenu,
                                ShowOnHomePage = item.Tool.ShowOnHomepage
                            }).Skip(toSkip).Take(1).First();
                        }
                    }
                }
            }

            if (toolDetail != null)
            {
                var featuredImage = toolDetail.FeaturedImage;
                toolDetail.FeaturedImage = !string.IsNullOrEmpty(featuredImage) ? _s3BucketService.RetrieveImageCDNUrl(featuredImage) : string.Empty;
                toolDetail.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage, EntityType.Tools);
            }

            if ( dbMedia.MediaResourceOrder.Count > 0 && !string.IsNullOrEmpty(dbMedia.MediaResourceOrder.FirstOrDefault().ResourceIds.Trim()) )
            {
                    var dbMediaResource = dbMedia.MediaResourceOrder.FirstOrDefault();
                var logMediaResource = new { dbMediaResource.Id, Mediaid = dbMediaResource.MediaId, dbMediaResource.ResourceIds };
                    var jsonString = LogsandException.GetCurrentInputJsonString(logMediaResource);
                    _logger.LogInformation($"Get:Received request for Media service and action:GetMediaAsync db data details:{jsonString}");              

                var resourceIds = dbMedia.MediaResourceOrder.FirstOrDefault().ResourceIds.Trim().Split(",").ToList();
                //var mediaResourceIds = resourceIds.ConvertAll(int.Parse);
                var mos = 0;
                var mediaResourceIds = resourceIds.Where(m => int.TryParse(m , out mos)).Select(m => int.Parse(m)).ToList();
                if ( mediaResourceIds.Count > 0)
                {
                    tools = _context.Tool.AsEnumerable().Where(t => mediaResourceIds.Contains(t.Id)).Select(x => new ShortToolModel
                    {
                        Id = x.Id ,
                        ToolName = x.Name , 
                        Url = x.Url ,
                        Description = x.Description ,
                        FeaturedImage = x.FeaturedImage ,
                        ShowOnMenu = x.ShowOnMenu ,
                        ShowOnHomePage = x.ShowOnHomepage
                    }).OrderBy(d => mediaResourceIds.IndexOf(d.Id)).ToList();
                }
            }
            else
            {
                tools = dbMedia.ToolMedia.Select(item => new ShortToolModel()
                {
                    Id = item.Tool.Id,
                    ToolName = item.Tool.Name,
                    Url = item.Tool.Url,
                    Description = item.Tool.Description,
                    FeaturedImage = item.Tool.FeaturedImage,
                    ShowOnMenu = item.Tool.ShowOnMenu,
                    ShowOnHomePage = item.Tool.ShowOnHomepage
                }).AsEnumerable().Union(dbMedia.ToolMedia.Select(item => new ShortToolModel()
                                 {
                                     Id = item.Tool.Id ,
                                     ToolName = item.Tool.Name ,
                                     Url = item.Tool.Url ,
                                     Description = item.Tool.Description ,
                                     FeaturedImage = item.Tool.FeaturedImage ,
                                     ShowOnMenu = item.Tool.ShowOnMenu ,
                                     ShowOnHomePage = item.Tool.ShowOnHomepage
                                 })).ToList();
                tools = tools.GroupBy(p => p.Id).Select(grp => grp.FirstOrDefault()).ToList();
            }
            foreach (var tool in tools)
            {
                var featuredImage = tool.FeaturedImage;
                tool.FeaturedImage = !string.IsNullOrEmpty(featuredImage) ? _s3BucketService.RetrieveImageCDNUrl(featuredImage) : string.Empty;
                tool.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage, EntityType.Tools);
            }

            var lstMediaAnnotations = new List<MediaAnnotationModel>();
            if(dbMedia.MediaAnnotation.Count > 0)
            {
                foreach ( var item in dbMedia.MediaAnnotation )
                {
                    lstMediaAnnotations.Add(new MediaAnnotationModel
                    {
                        TimeStamp = item.StartAt.ToString() ,
                        Duration = item.Duration ,
                        Text = item.Title ,
                        TypeId = item.TypeId ,
                        ResourceId = item.ResourceId ,
                        Link = item.Link
                    });
                }
            }

            var mediaTools = tools.Select(x => x.ToolName).ToList();
            var result =   new MediaViewModel
                                 {
                                     Id = dbMedia.Id ,
                                     Name = dbMedia.Name ,
                                     Description = dbMedia.Description ,
                                     LongDescription = dbMedia.LongDescription ,
                                     Url = _appSettings.IsHlsFormatEnabled == true && dbMedia.MediatypeId == 1 ? dbMedia.HlsUrl : dbMedia.Url ,
                                     EmbeddedCode = dbMedia.EmbeddedCode ,
                                     Logo = dbMedia.FeaturedImage ,
                                     ImageFileName = dbMedia.FeaturedImageMetadata ,
                                     Thumbnail = dbMedia.Thumbnail ,
                                     FeaturedImage = dbMedia.FeaturedImage ,
                                     MediaType = dbMedia.Mediatype?.Name ,
                                     MediaStatus = dbMedia.Mediastatus?.Name ,
                                     PublishDate = dbMedia.DatePublishedUtc ,
                                     CreatedDate = dbMedia.DateCreatedUtc ,
                                     Series = dbMedia.Series?.Name,
                                     SeriesId = dbMedia.Series != null ? dbMedia.Series.Id : 0 ,
                                     Topic = dbMedia.MediaTopic.Select(x => x.Topic.Name).ToList() ,
                                     Source = dbMedia.Source?.Name ,
                                     SourceId = dbMedia.Source != null ? dbMedia.Source.Id : 0 ,
                                     Tags = dbMedia.MediaTag.Select(x => x.Tag.Name).ToList() ,
                                     MediaAnnotations = lstMediaAnnotations,
                                     IsPrivate = dbMedia.IsPrivate ,
                                     IsSharingAllowed = (bool)dbMedia.IsSharingAllowed ,
                                     ActiveFromUtc = dbMedia.ActiveFromUtc ,
                                     ActiveToUtc = dbMedia.ActiveToUtc ,
                                     MediaTypeId = dbMedia.MediatypeId ,
                                     MediaMetaData = dbMedia.Metadata ,
                                     PublishedById = dbMedia.PublishUserId ,
                                     PublishedBy = dbMedia.PublishUser?.Name ,
                                     UploadedById = dbMedia.UploadUserId ,
                                     UploadedByUser = dbMedia.UploadUser?.Name , 
                                     SrtFile = dbMedia.SrtFile ,
                                     SrtFileName = dbMedia.SrtFileMetadata ,
                                     SeoUrl = dbMedia.SeoUrl ,
                                     DraftMediaSeoUrl = dbMedia.DraftMediaSeoUrl ,
                                     LastUpdatedDate = dbMedia.DateLastupdatedUtc ,
                                     IsVisibleOnGoogle = dbMedia.IsVisibleOnGoogle ,
                                     UniqueId = dbMedia.UniqueId
                                 };

            if (result != null)
            {
                result.TopicIds = dbMedia.MediaTopic.Select(x => x.TopicId).ToList();
                result.Tool = toolDetail;
                result.Tools = tools;
                result.MediaTools = mediaTools;

                var logo = result.Logo;
                result.Logo = !string.IsNullOrEmpty(logo) ? _s3BucketService.RetrieveImageCDNUrl(logo) : string.Empty;
                result.Logos = await _s3BucketService.GetCompressedImages(logo, EntityType.Media);

                var thumbnail = result.Thumbnail;
                result.Thumbnail = !string.IsNullOrEmpty(thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(thumbnail) : string.Empty;
                result.Thumbnails = await _s3BucketService.GetCompressedImages(thumbnail, EntityType.Media);

                var featuredImage = result.FeaturedImage;
                result.FeaturedImage = !string.IsNullOrEmpty(featuredImage) ? _s3BucketService.RetrieveImageCDNUrl(featuredImage) : string.Empty;
                result.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage, EntityType.Media);

                // get srtFile
                if (dbMedia.MediaSrt.Count > 0)
                {
                    var lstSrt = new List<SrtFileModel>();
                    foreach (var item in dbMedia.MediaSrt)
                    {
                        var srtFileDetail = new SrtFileModel
                        {
                            SrtFile = "",
                            SrtFileName = item.FileMetadata,
                            Uuid = item.File,
                            SrtLanguage = item.Language,
                            PreSignedUrl = item != null ? _s3BucketService.RetrieveImageCDNUrl(item.File) : string.Empty
                        };
                        lstSrt.Add(srtFileDetail);
                    }
                    result.LstSrtFile = lstSrt;
                }
                else
                {
                    result.LstSrtFile = null;
                }
            }
            return result;
        }

        public async Task<List<MediaInfoModel>> GetMediaCarouselInfoAsync(IdsRequest<long> model)
        {
            var result = await _context.Media.Where(x => model.Ids.Contains(x.Id)).Select(item =>
                new MediaInfoModel
                {
                    Id = item.Id,
                    Title = item.Name,
                    Url = item.Url,
                    IsSharingAllowed = item.IsSharingAllowed,
                    Thumbnail = item.FeaturedImage,
                    Logo = item.FeaturedImage,
                    SeoUrl = item.SeoUrl,
                    IsVisibleOnGoogle = item.IsVisibleOnGoogle
                }).ToListAsync();
           
            if (result != null && result.Any())
            {
                return result;
            }
            return null;
        }

        public async Task<List<MediaShortModel>> GetMediaShortInfoAsync()
        {
            var medias = await _context.Media.Where(media => media.MediastatusId == (int)MediaStatusEnum.Published && !media.IsDeleted).Select(
                   item =>
                       new MediaShortModel { Id = item.Id, Name = item.Name, MediaTypeId = item.MediatypeId }).ToListAsync();
            if (medias.Count > 0)
            {
                foreach (var media in medias)
                {
                    media.IsActive = IsMediaActive(media.Id);
                }
                return medias;
            }
            return null;
        }

        public async Task<GridResponse<MediaAutoCompleteModel>> MediaAutoCompleteSearch()
        {
            var mediaNames = _context.Media.Where(y=>!y.IsDeleted).Select(x => new MediaAutoCompleteModel { Name = x.Name }).ToList();
            var mediaIds = _context.Media.Where(y=>!y.IsDeleted).Select(x => new MediaAutoCompleteModel { Name = x.Id.ToString() }).ToList();
            var items = mediaNames.Union(mediaIds, new MediaComparer()).OrderBy(x => x.Name).ToList();
            return new GridResponse<MediaAutoCompleteModel> { items = items, TotalRowsAvailable = items.Count };
        }

        public async Task<GridResponse<MediaListResponse>> MediaSearch(FilterMediaRequest request)
        {
            var items = new List<MediaListResponse>();
            var totalMatchingCount = 0;
            if ((!string.IsNullOrEmpty(request.Filterstring)) || (!string.IsNullOrEmpty(request.Source)))
            {
                var fields = _cloudMediaSearchProvider.SearchFromCloud(request.Filterstring, request.PageSize, request.Page, request.Status, request.MediaType, request.PublishFromDate, request.PublishToDate, request.Source, request.TopicName, request.SeriesName, request.UploadUser, request.PublishUser, request.SortOrder, request.SortedProperty, out var count); // testing cloud
                items = (from x in fields
                         let toolMedia = (from tool_Media in _context.ToolMedia
                                          join tool in _context.Tool on tool_Media.ToolId equals tool.Id
                                          where tool_Media.MediaId == long.Parse(x.id)
                                          select tool.Name)
                                   .Union(from tool_Series in _context.ToolSeries
                                          join media in _context.Media on tool_Series.SeriesId equals media.SeriesId
                                          join tool in _context.Tool on tool_Series.ToolId equals tool.Id
                                          where media.Id == long.Parse(x.id)
                                          select tool.Name)
                                   .ToList()
                         select new MediaListResponse
                         {
                             Id = Convert.ToInt32(x.id),
                             Name = x.title,
                             Description = x.description,
                             Topic = x.topictitle,
                             MediaType = x.mediatype,
                             MediaStatus = x.status,
                             PublishedBy = x.publisedby,
                             Series = x.seriestitle,
                             Source = x.source,
                             MediaTools = toolMedia,
                             PublishDate = Helper.ConvertMomentToDateTime(x.date),
                             UploadedByUser = x.uploadedby,
                             IsVisibleOnGoogle = Convert.ToBoolean(x.isvisibleongoogle)
                         }).ToList();

                totalMatchingCount = count;
                _logger.LogDebug($"Search:fulfilled request controller: MediaController and action: MediaSearch 'search from cloud' with FilterString: {request.Filterstring} and source is : {request.Source}");

            }
            else
            {

                var skip = request.PageSize * (request.Page - 1);

                var mediaQuery =   (from media in _context.Media.
                                     Include(x => x.MediaTopic)
                                    .ThenInclude(x => x.Topic)
                                    .Include(x => x.ToolMedia)
                                    .ThenInclude(x => x.Tool)
                                    .Include(x => x.Mediastatus)
                                    .Include(x => x.Mediatype)
                                    .Include(x => x.PublishUser)
                                    .Include(x => x.UploadUser)
                                    .Include(x => x.Series)
                                    .ThenInclude(x => x.ToolSeries)
                                    .ThenInclude(x => x.Tool)
                                    .Include(x => x.Source).Where(x => !x.IsDeleted).AsQueryable()                                   
                                    select new 
                         {
                             Id = media.Id,
                             Name = media.Name,
                             Description = media.Description,
                             Topic = media.MediaTopic.Select(x => x.Topic.Name),
                             Media_Tools = media.ToolMedia.Select(x => x.Tool.Name),
                             Series_Tools = media.Series.ToolSeries.Select(x => x.Tool.Name),  
                             MediaType = media.Mediatype.Name,
                             MediaStatus = media.Mediastatus.Name,
                             PublishedBy = media.PublishUser.Name,
                             Series = media.Series.Name,
                             Source = media.Source.Name,
                             PublishDate = media.DatePublishedUtc,
                             UploadedByUser = media.UploadUser.Name,
                             SeoUrl = media.SeoUrl,
                             IsVisibleOnGoogle = media.IsVisibleOnGoogle
                         }).OrderByDescending(p => p.PublishDate).AsQueryable();

                _logger.LogDebug($"Search:fulfilled request controller: MediaController and action: MediaSearch 'search from DB' with empty FilterString: {request.Filterstring} and empty source is : {request.Source}");

                if (request.PublishFromDate.HasValue && request.PublishToDate.HasValue)
                {
                    mediaQuery = mediaQuery.Where(item => item.PublishDate.HasValue && item.PublishDate.Value >= request.PublishFromDate && item.PublishDate.Value.Date <= request.PublishToDate);
                }

                if (!string.IsNullOrEmpty(request.Status))
                {
                    if (request.Status.ToLower() != "all")
                        mediaQuery = mediaQuery.Where(item => item.MediaStatus == request.Status);
                }
                if (!string.IsNullOrEmpty(request.MediaType))
                {
                    if (request.MediaType.ToLower() != "all")
                        mediaQuery = mediaQuery.Where(item => item.MediaType == request.MediaType);
                }
                if (!string.IsNullOrEmpty(request.Source))
                {
                    if (request.Source.ToLower() != "all")
                        mediaQuery = mediaQuery.Where(item => item.Source == request.Source);
                }

                if (!string.IsNullOrEmpty(request.TopicName))
                {
                    if (request.TopicName.ToLower() != "all")
                        mediaQuery = mediaQuery.Where(item => item.Topic.Contains(request.TopicName));
                }
                if (!string.IsNullOrEmpty(request.SeriesName))
                {
                    if (request.SeriesName.ToLower() != "all")
                        mediaQuery = mediaQuery.Where(item => item.Series == request.SeriesName);
                }
                if (!string.IsNullOrEmpty(request.UploadUser))
                {
                    if (request.UploadUser.ToLower() != "all")
                        mediaQuery = mediaQuery.Where(item => item.UploadedByUser == request.UploadUser);
                }
                if (!string.IsNullOrEmpty(request.PublishUser))
                {
                    if (request.PublishUser.ToLower() != "all")
                        mediaQuery = mediaQuery.Where(item => item.PublishedBy == request.PublishUser);
                }
                //totalMatchingCount = items.Count;
                //sorting
                if (!string.IsNullOrEmpty(request.SortedProperty) && !string.IsNullOrEmpty(request.SortOrder))
                {
                    _logger.LogDebug($"fulfilled request controller: MediaController and action: MediaSearch for sorted property: {request.SortedProperty} and sorted property is : {request.SortOrder}");


                    if (request.SortOrder.ToLower() == "descending" && request.SortedProperty.ToLower() == "id")
                    {
                        mediaQuery = mediaQuery.OrderByDescending(item => item.Id);
                    }
                    if (request.SortOrder.ToLower() == "ascending" && request.SortedProperty.ToLower() == "id")
                    {
                        mediaQuery = mediaQuery.OrderBy(item => item.Id);
                    }

                    if (request.SortOrder.ToLower() == "descending" && request.SortedProperty.ToLower() == "title")
                    {
                        mediaQuery = mediaQuery.OrderByDescending(item => item.Name);
                    }
                    if (request.SortOrder.ToLower() == "ascending" && request.SortedProperty.ToLower() == "title")
                    {
                        mediaQuery = mediaQuery.OrderBy(item => item.Name);
                    }
                    if (request.SortOrder.ToLower() == "descending" && request.SortedProperty.ToLower() == "status")
                    {
                        mediaQuery = mediaQuery.OrderByDescending(item => item.MediaStatus);
                    }
                    if (request.SortOrder.ToLower() == "ascending" && request.SortedProperty.ToLower() == "status")
                    {
                        mediaQuery = mediaQuery.OrderBy(item => item.MediaStatus);
                    }
                    if (request.SortOrder.ToLower() == "descending" && request.SortedProperty.ToLower() == "mediatype")
                    {
                        mediaQuery = mediaQuery.OrderByDescending(item => item.MediaType);
                    }
                    if (request.SortOrder.ToLower() == "ascending" && request.SortedProperty.ToLower() == "mediatype")
                    {
                        mediaQuery = mediaQuery.OrderBy(item => item.MediaType);
                    }
                    if (request.SortOrder.ToLower() == "descending" && request.SortedProperty.ToLower() == "date")
                    {
                        mediaQuery = mediaQuery.OrderByDescending(item => item.PublishDate);
                    }
                    if (request.SortOrder.ToLower() == "ascending" && request.SortedProperty.ToLower() == "date")
                    {
                        mediaQuery = mediaQuery.OrderBy(item => item.PublishDate);
                    }

                    if (request.SortOrder.ToLower() == "descending" && request.SortedProperty.ToLower() == "source")
                    {
                        mediaQuery = mediaQuery.OrderByDescending(item => item.Source);
                    }
                    if (request.SortOrder.ToLower() == "ascending" && request.SortedProperty.ToLower() == "source")
                    {
                        mediaQuery = mediaQuery.OrderBy(item => item.Source);
                    }
                    if (request.SortOrder.ToLower() == "descending" && request.SortedProperty.ToLower() == "uploadedby")
                    {
                        mediaQuery = mediaQuery.OrderByDescending(item => item.UploadedByUser);
                    }
                    if (request.SortOrder.ToLower() == "ascending" && request.SortedProperty.ToLower() == "uploadedby")
                    {
                        mediaQuery = mediaQuery.OrderBy(item => item.UploadedByUser);
                    }

                    if (request.SortOrder.ToLower() == "descending" && request.SortedProperty.ToLower() == "publisedby")
                    {
                        mediaQuery = mediaQuery.OrderByDescending(item => item.PublishedBy);
                    }
                    if (request.SortOrder.ToLower() == "ascending" && request.SortedProperty.ToLower() == "publisedby")
                    {
                        mediaQuery = mediaQuery.OrderBy(item => item.PublishedBy);
                    }

                }

                totalMatchingCount = await mediaQuery.CountAsync();

                items = await mediaQuery.Select(media => new MediaListResponse
                {
                    Id = media.Id,
                    Name = media.Name,
                    Description = media.Description,
                    Topic = media.Topic.ToList(),
                    MediaTools = media.Media_Tools.AsEnumerable().Union(media.Series_Tools).ToList(),
                    MediaType = media.MediaType,
                    MediaStatus = media.MediaStatus,
                    PublishedBy = media.PublishedBy,
                    Series = media.Series,
                    Source = media.Source,
                    PublishDate = media.PublishDate,
                    UploadedByUser = media.UploadedByUser,
                    SeoUrl = media.SeoUrl,
                    IsVisibleOnGoogle = media.IsVisibleOnGoogle
                }).ToListAsync();

                if (request.Page != 0 && request.PageSize >= 0)
                {
                    items = items.Skip(skip).Take(request.PageSize).ToList();
                }
            }
            return new GridResponse<MediaListResponse> { items = items, TotalRowsAvailable = totalMatchingCount };
        }

        public async Task<CollectionModel<MediaInfoModel>> MediaPlayListAsync(int playlistId)
        {
            var mediaList = await GetMediaPlayListAsync(playlistId);
            if (mediaList != null)
            {
                foreach (var media in mediaList)
                {
                    var thumbnail = media.Thumbnail;
                    media.Thumbnail = !string.IsNullOrEmpty(media.Thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(media.Thumbnail) : string.Empty;
                    media.Thumbnails = await _s3BucketService.GetCompressedImages(thumbnail, EntityType.Media);

                    var logo = media.Logo;
                    media.Logo = !string.IsNullOrEmpty(media.Logo) ? _s3BucketService.RetrieveImageCDNUrl(media.Logo) : string.Empty;
                    media.Logos = await _s3BucketService.GetCompressedImages(logo, EntityType.Media);
                }

                return new CollectionModel<MediaInfoModel> { Items = mediaList, TotalCount = mediaList.Count };
            }

            return new CollectionModel<MediaInfoModel>();
        }

        #region Private Methods
        /// <summary>
        /// Check media status
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        private bool IsMediaActive(long mediaId)
        {
            var media = _context.Media.FirstOrDefault(x => x.Id == mediaId);

            if (media != null)
            {
                if (media.ActiveFromUtc != null && media.ActiveToUtc != null)
                    return media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= media.ActiveToUtc;
                else if (media.ActiveFromUtc != null && media.ActiveToUtc == null)
                    return media.ActiveFromUtc <= DateTime.UtcNow;
                else if (media.ActiveFromUtc == null && media.ActiveToUtc != null)
                    return DateTime.UtcNow <= media.ActiveToUtc;
                else
                    return true;
            }
            return false;
        }
        private async Task<List<MediaInfoModel>> GetMediaPlayListAsync(int playlistId)
        {

            var result = ( from plmedia in _context.PlaylistMedia.Include(x => x.Media)
                           where plmedia.PlaylistId == playlistId && plmedia.Media.MediastatusId != (int)MediaStatusEnum.Archived
                           && plmedia.Media.MediastatusId != (int)MediaStatusEnum.Draft && !plmedia.Media.IsDeleted
                           && ( plmedia.Media.ActiveFromUtc.HasValue &&  plmedia.Media.ActiveToUtc.HasValue ? plmedia.Media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= plmedia.Media.ActiveToUtc :
                                           plmedia.Media.ActiveFromUtc.HasValue && !plmedia.Media.ActiveToUtc.HasValue ? plmedia.Media.ActiveFromUtc <= DateTime.UtcNow :
                                          plmedia.Media.ActiveFromUtc.HasValue || !plmedia.Media.ActiveToUtc.HasValue || DateTime.UtcNow <= plmedia.Media.ActiveToUtc )
                           select new MediaInfoModel
                           {
                               Id = plmedia.Media.Id ,
                               Title = plmedia.Media.Name ,
                               Url = plmedia.Media.Url ,
                               Logo = plmedia.Media.FeaturedImage ,
                               Thumbnail = plmedia.Media.FeaturedImage ,
                               IsSharingAllowed = plmedia.Media.IsSharingAllowed ,
                               SeoUrl = plmedia.Media.SeoUrl ,
                               UniqueId = plmedia.Media.UniqueId ,
                           } ).ToList();
            return result;
        }
        #endregion
    }
}
