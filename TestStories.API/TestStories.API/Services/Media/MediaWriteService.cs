using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestStories.API.Common;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.API.Services;
using TestStories.CloudSearch.Service.Interface;
using TestStories.CloudSearch.Service.MediaEntity;
using TestStories.Common;
using TestStories.Common.Configurations;
using TestStories.Common.Events;
using TestStories.Common.MailKit;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;
using Newtonsoft.Json;

namespace TestStories.API.Concrete
{
    public class MediaWriteService : IMediaWriteService
    {
        private readonly TestStoriesContext _context;
        private readonly ILogger<MediaWriteService> _logger;
        private readonly ICloudMediaSearchProvider _cloudMediaSearchProvider;
        private readonly IPublishEvent<SendEmail> _event;
        private readonly EmailSettings _emailSettings;
        private readonly IS3BucketService _s3BucketService;

        public MediaWriteService(TestStoriesContext ctx, ILogger<MediaWriteService> logger, ICloudMediaSearchProvider cloudMediaSearchProvider, IS3BucketService s3BucketService,
            IPublishEvent<SendEmail> eEvent, 
            IOptions<EmailSettings> emailSettings )
        {
            _context = ctx;
            _logger = logger;
            _cloudMediaSearchProvider = cloudMediaSearchProvider;
            _event = eEvent;
            _emailSettings = emailSettings.Value;
            _s3BucketService = s3BucketService;
        }

        public async Task<BaseResponse> UpdateMediaStatus(int mediaId, byte statusId)
        {
            BaseResponse response;
            var media = _context.Media.SingleOrDefault(med => med.Id == mediaId);
            if (media != null)
            {
                media.MediastatusId = statusId;
                _context.Media.Update(media);
                await _context.SaveChangesAsync();
                response = new BaseResponse() { ErrorDescription = "Success", ErrorCode = 200 };
            }
            else
            {
                response = new BaseResponse() { ErrorDescription = "NotFound", ErrorCode = 404 };
            }

            return response;
        }

        public async Task DeleteMediaByTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException("Media title cannot be empty");
            }

            var media = _context.Media.Include(x => x.MediaSrt).FirstOrDefault(x => x.Name.Trim() == title.Trim());
            if (media == null)
            {
                throw new ArgumentException("Media not found");
            }

            _context.Media.Remove(media);
            await _context.SaveChangesAsync();
            _cloudMediaSearchProvider.DeleteFromCloud(media.Id);


            if (!string.IsNullOrEmpty(media.Url))
            {
                await _s3BucketService.RemoveImageAsync(media.Url);
            }

            if (!string.IsNullOrEmpty(media.FeaturedImage))
            {
                await _s3BucketService.RemoveImageAsync(media.FeaturedImage);
            }

            if (!string.IsNullOrEmpty(media.Thumbnail))
            {
                await _s3BucketService.RemoveImageAsync(media.Thumbnail);
            }
        }
     
        public async Task MigrateSrtFiles(List<AddSrtFileModel> srtFiles)
        {

            var newSrtFiles = new List<MediaSrt>();

            foreach (var item in srtFiles)
            {
                var isSrtExist = _context.MediaSrt.Any(x => x.MediaId == item.MediaId && x.File == item.File);
                if (!isSrtExist)
                {
                    newSrtFiles.Add(new MediaSrt { File = item.File, FileMetadata = item.FileMetadata, Language = item.Language, MediaId = item.MediaId });
                }
            }

            if (newSrtFiles.Count > 0)
            {
                _logger.LogDebug($"Request received Controller:EfMediaRepository and Action:migrateSrtFiles, MediaIds: {  string.Join(", ", newSrtFiles.Select(x => x.MediaId))}");
                _context.MediaSrt.AddRange(newSrtFiles);
                await _context.SaveChangesAsync();
            }
        }

        public async Task GenerateSeoFriendlyUrl(bool isAllUpdate)
        {
            var medias = new List<Media>();
            var topics = new List<Topic>();
            var series = new List<Series>();
            if (isAllUpdate)
            {
                medias = _context.Media.ToList();
                topics = _context.Topic.ToList();
                series = _context.Series.ToList();
            }
            else
            {
                medias = _context.Media.Where(x => x.SeoUrl == null || x.SeoUrl == "").ToList();
                topics = _context.Topic.Where(x => x.SeoUrl == null || x.SeoUrl == "").ToList();
                series = _context.Series.Where(x => x.SeoUrl == null || x.SeoUrl == "").ToList();
            }

            Parallel.ForEach(medias, async media =>
            {
                media.SeoUrl = Helper.SeoFriendlyUrl(media.Name);
            });

            _context.Media.UpdateRange(medias);


            Parallel.ForEach(topics, async topic =>
            {
                topic.SeoUrl = Helper.SeoFriendlyUrl(topic.Name);
            });
            _context.Topic.UpdateRange(topics);


            Parallel.ForEach(series, async serie =>
            {
                serie.SeoUrl = Helper.SeoFriendlyUrl(serie.Name);
            });
            _context.Series.UpdateRange(series);
            _context.SaveChanges();
        }

        public async Task GenerateHlsUrl()
        {
            var medias = await _context.Media.Where(x => x.MediatypeId == (int)MediaTypeEnum.Video
                          && (x.HlsUrl == null || x.HlsUrl == "")).ToListAsync();

            Parallel.ForEach(medias, async media =>
           {
               media.HlsUrl = $"{ media.Url.Split('.').First() }/hlsv3/index.m3u8";
           });
            _context.Media.UpdateRange(medias);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MediaSeoDetailModel>> GenerateNewSeoUrl()
        {
            var medias = new List<MediaSeoDetailModel>();
            var query = await (from media in _context.Media where !string.IsNullOrEmpty(media.Metadata) select media).ToListAsync();
            foreach (var item in query)
            {
                var metaData = JsonConvert.DeserializeObject<MediaMetaData>(item.Metadata);
                if (!metaData.name.Contains(item.Name))
                {
                    if (Helper.SeoFriendlyUrl(metaData.name).Contains(item.SeoUrl))
                    {
                        medias.Add(new MediaSeoDetailModel { Id = item.Id, Title = item.Name, OldSeoUrl = item.SeoUrl, NewSeoUrl = Helper.SeoFriendlyUrl(item.Name), MediaMetaData = item.Metadata });
                    }
                }
            }

            return medias;
        }

        public async Task UpdateMediaSeoUrl()
        {
            var result = await GenerateNewSeoUrl();
            foreach (var item in result)
            {
                var media = await UpdateMedia(item.Id, item.NewSeoUrl);
                if (media != null)
                {
                    UpdateCloudMedia(media.Id);
                }
            }
        }

        public async Task<List<MediaSeoDetailModel>> GetUpdatedSeoUrl()
        {
            var medias = await (from media in _context.Media
                                where !string.IsNullOrEmpty(media.SeoUrl)
                                select new MediaSeoDetailModel
                                {
                                    Id = media.Id,
                                    Title = media.Name,
                                    NewSeoUrl = media.SeoUrl,
                                    MediaMetaData = media.Metadata
                                }).OrderBy(x => x.Id).ToListAsync();
            return medias;
        }

        public async Task<BaseResponse> ArchiveMediaAsync(int mediaId, int userId, string role)
        {
            if (role == "Partner-User")
            {
                var userPartnerId = _context.User.FirstOrDefault(x => x.Id == userId).PartnerId ;
                var mediaPartnerId =  _context.Media.FirstOrDefault(x => x.Id == mediaId).SourceId;

                if (userPartnerId != mediaPartnerId)
                    throw new UnauthorizedAccessException("You are not permitted to update this entity because of user role restrictions");

            }
            var response = await ArchiveMedia(mediaId);
            _logger.LogDebug($"Set media status is Archived on DB");
            // updating on Cloud from DB
            if (response.ErrorCode != 404)
            {
                var media = _cloudMediaSearchProvider.GetCurrentItemFromCloud(mediaId.ToString(), 10, 1).FirstOrDefault();
                if (media != null)
                {
                    var clousearchdentity = new MediaCloudSearchEntity
                    {
                        Id = Convert.ToInt64(media.id),
                        Title = media.title,
                        Description = media.description,
                        LongDescription = media.longdescription,
                        SeriesTitle = media.seriestitle,
                        TopicTitle = media.topictitle,
                        Tags = media.tags ?? new List<string>(),
                        Status = MediaStatusEnum.Archived.ToString(), //"Archived",
                        MediaType = media.mediatype,
                        Date = media.date,
                        Source = media.source,
                        UploadedBy = media.uploadedby,
                        PublishedBy = media.publisedby,
                        Logo = media.logo,
                        IsPrivate = media.isprivate,
                        ActiveFrom = media.activefrom,
                        ActiveTo = media.activeto,
                        LastUpdatedDate = DateTime.UtcNow,
                        IsSharingAllowed = media.issharingallowed,
                        thumbnail = media.thumbnail,
                        seourl = media.seourl,
                        UniqueId = media.uniqueid,
                        IsDeleted = media.isdeleted,
                        IsVisibleOnGoogle = media.isvisibleongoogle
                    };
                    var status = _cloudMediaSearchProvider.UpdateToCloud(clousearchdentity);
                    if (status.ToLower().Trim() != CloudStatus.success.ToString())
                    {
                        _logger.LogError($"Update:Controller:MediaController and Action:ArchiveMedia failure. {status}");
                    }
                }
            }

            return response;
        }

        public async Task<BaseResponse> UnarchiveMediaAsync(int mediaId, int userId, string role)
        {
            if (role == "Partner-User")
            {
                var userPartnerId = _context.User.FirstOrDefault(x => x.Id == userId).PartnerId;
                var mediaPartnerId = _context.Media.FirstOrDefault(x => x.Id == mediaId).SourceId;

                if (userPartnerId != mediaPartnerId)
                    throw new UnauthorizedAccessException("You are not permitted to update this entity because of user role restrictions");

            }

            var partnerId = _context.Media.FirstOrDefault(x => x.Id == mediaId).SourceId;
            var isparnerArchived = partnerId != null && _context.Partner.FirstOrDefault(x => x.Id == partnerId).IsArchived;
            if(isparnerArchived)
            {
                throw new BusinessException("The partner is archived for this media.");
            }

            var response = await UnarchiveMedia(mediaId);
            if (response.ErrorCode != 404)
            {
                var media = _cloudMediaSearchProvider.GetCurrentItemFromCloud(mediaId.ToString(), 10, 1).FirstOrDefault();
                if (media != null)
                {

                    var clousearchdentity = new MediaCloudSearchEntity
                    {
                        Id = Convert.ToInt64(media.id),
                        Title = media.title,
                        Description = media.description,
                        LongDescription = media.longdescription,
                        SeriesTitle = media.seriestitle,
                        TopicTitle = media.topictitle,
                        Tags = media.tags ?? new List<string>(),
                        Status = MediaStatusEnum.Draft.ToString(), //"Draft",
                        MediaType = media.mediatype,
                        Date = "",
                        Source = media.source,
                        UploadedBy = media.uploadedby,
                        PublishedBy = "",
                        Logo = media.logo,
                        IsPrivate = media.isprivate,
                        ActiveFrom = media.activefrom,
                        ActiveTo = media.activeto,
                        LastUpdatedDate = DateTime.UtcNow,
                        IsSharingAllowed = media.issharingallowed,
                        thumbnail = media.thumbnail,
                        seourl = media.seourl,
                        UniqueId = media.uniqueid,
                        IsDeleted = media.isdeleted,
                        IsVisibleOnGoogle = media.isvisibleongoogle
                    };
                    var status = _cloudMediaSearchProvider.UpdateToCloud(clousearchdentity);
                    if (status.ToLower().Trim() != CloudStatus.success.ToString())
                    {
                        _logger.LogDebug($"Set media status is Draft on Cloud for mediaId {status}");
                    }
                    // If loggedIn user as Partner-User then send mail to him 
                    var user = _context.User.Include(x => x.Usertype).Where(x => x.Id == userId).FirstOrDefault();
                    if (!string.IsNullOrEmpty(user.Usertype.Name) && user.Usertype.Name == "Partner-User")
                    {
                        // Send Mail to Admin
                        PartnerMediaReviewMail(user.Name, mediaId);
                    }
                }
            }
            return response;
        }

        public async Task<MediaShortModel> AddMediaAsync(AddMediaModel model, int userId)
        {
            Media media;
            //string[] lstTags = null;
            var hlsUrl = string.Empty;

            var jsonString = LogsandException.GetCurrentInputJsonString(model);
          
            var alreadyExists = await _context.Media.Where(x => x.Name.Trim() == model.Title.Trim()).AnyAsync();
            if (alreadyExists)
            {
                throw new BusinessException("Error creating media - Title is duplicate. Please select a different title");
            }

            if (model.MediaTypeId != 0)
            {
                if (model.MediaTypeId == (int)MediaTypeEnum.Video || model.MediaTypeId == (int)MediaTypeEnum.PodcastAudio)
                {
                    if (string.IsNullOrEmpty(model.Url))
                    {
                        _logger.LogDebug($"Add: Received: request for Controller:MediaController and Action: AddMediaAsync The incorrect media url to add media.");
                        throw new BusinessException("The incorrect media url to add media.");
                    }
                }
                else if (model.MediaTypeId == (int)MediaTypeEnum.EmbeddedMedia)
                {
                    if (string.IsNullOrEmpty(model.EmbeddedCode))
                    {
                        _logger.LogDebug($" Add: Received: request for Controller:MediaController and Action: AddMediaAsync The incorrect media embedCode to add media.");
                        throw new BusinessException("The incorrect media embedCode to add media.");
                    }
                }
            }
            else
            {
                throw new BusinessException("The incorrect media type to add embed media.");
            }

            if (!string.IsNullOrEmpty(model.Url))
            {
                var mediaType = model.Url.Split('.').Last();
                string[] videoType = { "mov", "mp4", "m4v", "webm", "ogv", "mpg", "mpeg" };
                if (videoType.Contains(mediaType))
                {
                    model.Url = model.Url.Split('.').First() + ".mp4";
                    hlsUrl = $"{ model.Url.Split('.').First() }/hlsv3/index.m3u8";
                }
            }
           
            var partnerUser = await _context.User.Include(x => x.Usertype).Where(x => x.Id == userId).FirstOrDefaultAsync();
            var isPartner = false;
            int? sourceId = null;
            if (!string.IsNullOrEmpty(partnerUser.Usertype.Name) && partnerUser.Usertype.Name == "Partner-User")
            {
                isPartner = true;
                sourceId = partnerUser.PartnerId;
            }

            media = await AddMedia(new Media
            {
                Name = model.Title,
                EmbeddedCode = model.EmbeddedCode ?? string.Empty ,
                Description =  model.Description ?? string.Empty ,
                LongDescription = model.LongDescription ,
                UploadUserId = userId,
                MediastatusId = model.MediaStatusId,
                MediatypeId = model.MediaTypeId,
                Url = model.Url ?? string.Empty , 
                HlsUrl = hlsUrl,
                Thumbnail = model.Thumbnail ?? string.Empty ,
                Metadata = model.MediaMetaData,
                SourceId = isPartner ? sourceId : model.SourceId,
                IsVisibleOnGoogle = model.IsVisibleOnGoogle
            });


            if (media != null)
            {
                var mediaModel = new MediaShortModel
                {
                    Id = media.Id,
                    Name = media.Name,
                };
                // Adding on Cloud
                // Get the data of media from db to update at cloud

                var mediaDetails = await _context.Media.Include(x => x.Mediastatus).Include(y => y.Mediatype).Include(z => z.Series)
                    .Include(m => m.PublishUser).Include(n => n.UploadUser).Include(o => o.Source).Where(p => p.Id == media.Id).FirstOrDefaultAsync();

                if ( mediaDetails != null )
                {
                    var clousearchdentity = new MediaCloudSearchEntity
                    {
                        Id = media.Id ,
                        Title = media.Name ,
                        Description = media.Description ,
                        LongDescription = media.LongDescription ,
                        SeriesTitle = mediaDetails.Series?.Name ,
                        TopicTitle = new List<string>() ,
                        Tags = new List<string>() ,
                        Status = mediaDetails.Mediastatus?.Name ,
                        MediaType = mediaDetails.Mediatype?.Name ,
                        Date = media.DatePublishedUtc.ToString() ,
                        Source = mediaDetails.Source?.Name ,
                        UploadedBy = mediaDetails.UploadUser?.Name ,
                        PublishedBy = mediaDetails.PublishUser?.Name,
                        Logo = media.FeaturedImage ,//media.Url
                        IsPrivate = Convert.ToInt32(media.IsPrivate) ,
                        IsSharingAllowed = Convert.ToInt32(media.IsSharingAllowed) ,
                        thumbnail = media.Thumbnail ,
                        seourl = media.SeoUrl ,
                        IsDeleted = Convert.ToInt32(media.IsDeleted) ,
                        UniqueId =  media.UniqueId ,
                        IsVisibleOnGoogle = Convert.ToInt32(media.IsVisibleOnGoogle)
                    };
                    var status = _cloudMediaSearchProvider.AddToCloud(clousearchdentity);
                    if ( status.ToLower().Trim() != CloudStatus.success.ToString() )
                    {
                        _logger.LogError($"Add: Received: request for Controller:MediaController and Action: AddMediaAsync cloud status {status}");
                    }
                }
                    if ( !string.IsNullOrEmpty(partnerUser.Usertype.Name) && partnerUser.Usertype.Name == "Partner-User" )
                    {
                        if ( media.MediatypeId == (int)MediaTypeEnum.EmbeddedMedia )
                        {
                            // Send Mail to Admin
                            PartnerMediaReviewMail(partnerUser.Name , media.Id);
                            _logger.LogDebug($"partner media review mail");
                        }
                    }
               
                return mediaModel;
            }
            throw new BusinessException("Can not add a new media. Please, try again.");
        }

        public async Task<MediaShortModel> EditMediaAsync(EditMediaModel model, int userId)
        {
            var mediaDetail = await _context.Media.Include(y => y.MediaSrt).SingleOrDefaultAsync(x => x.Id == model.Id);

            if (model.Url != null)
            {
                if (model.MediaStatusId == (byte)MediaStatusEnum.Published && model.Url.Split('.').LastOrDefault() == VideoFileTypeEnum.m3u8.ToString())
                {
                    var UUid = mediaDetail.Url.Split(".").FirstOrDefault();
                    UUid = UUid.Split("/").LastOrDefault();
                    _logger.LogDebug($"UUID: { UUid }");
                    var record = await DynamoDBService.FindById(UUid);
                    _logger.LogDebug($"Media in DynamoDb: { record }");
                    if (record != null)
                    {
                        if (record.Status != VideoPipelineStatusEnum.TranscodeSuccess)
                        {
                            throw new BusinessException("Transcoding in progress. Please try publishing media again after 10 mins.");
                        }
                    }
                }
            }

            if (mediaDetail is null)
            {
                throw new BusinessException("Media not exist");
            }

            var alreadyExists = await _context.Media.Where(x => x.Id != model.Id && x.Name.Trim() == model.Title.Trim()).AnyAsync();
            if (alreadyExists)
            {
                throw new BusinessException("Error creating media - Title is duplicate. Please select a different title");
            }

            if ( model.UniqueId != null )
            {
                var isUniqueIdExist = await _context.Media.Where(x => x.Id != model.Id && x.UniqueId.Trim() == model.UniqueId.Trim()).AnyAsync();
                if ( isUniqueIdExist )
                {
                    throw new BusinessException("Error creating media - UniqueId is duplicate. Please select a different Unique Id");
                }
                if(!Helper.IsValidUniqueId(model.UniqueId))
                {
                    throw new BusinessException("Error creating media - Spaces not allowed in Unique Id");
                }
            }

            var mediaAnnotations = model.MediaAnnotations != null ? JsonConvert.DeserializeObject<List<MediaAnnotationModel>>(model.MediaAnnotations) : null;
            var lstNewMediAnnotations = new List<MediaAnnotation>();
            if ( mediaAnnotations !=null && mediaAnnotations.Count > 0)
            {
                var isOverlap = IsTimespanOverlap(mediaAnnotations);
                if(isOverlap)
                {
                    throw new BusinessException("Error creating media - Timespan is overlapping. Please select a different time span");
                }

                if ( mediaAnnotations.Count > 0 )
                {
                    foreach ( var item in mediaAnnotations )
                    {
                        //if(item.ResourceId == null && string.IsNullOrEmpty(item.Link))
                       // {
                            //throw new BusinessException("Error updating media - Either Resource or Link mandatory. Please select any one");
                      //  }
                        if(item.TypeId.ToString() == "0")
                        {
                            lstNewMediAnnotations.Add(new MediaAnnotation
                            {
                                MediaId = model.Id ,
                                StartAt = TimeSpan.Parse(item.TimeStamp),
                                Duration = item.Duration ,
                                Title = item.Text ,
                                TypeId = item.TypeId ,
                                ResourceId = null ,
                                Link = null
                            });
                        }
                        else
                        {
                            lstNewMediAnnotations.Add(new MediaAnnotation
                            {
                                MediaId = model.Id ,
                                StartAt = TimeSpan.Parse(item.TimeStamp),
                                Duration = item.Duration ,
                                Title = item.Text ,
                                TypeId = item.TypeId ,
                                ResourceId = item.ResourceId ,
                                Link = item.Link
                            });
                        }           
                    }
                }
            }
               
            Media media;
            string[] lstTags = null;
            List<string> lstSrtUuids = null;
            List<string> lstSrtFileNames = null;
            List<string> lstOriginalSrtUuids = null;
            List<string> lstOriginalSrtFileNames = null;
            var mediaSrtItems = new List<MediaSrtItem>();
            var lstTopicIds = new List<int>();
            var hlsUrl = string.Empty;
            if (model.TopicIds != null)
            {
                var tIds = model.TopicIds.Split(',');
                foreach (var tId in tIds)
                {
                    lstTopicIds.Add(Convert.ToInt32(tId));
                }
            }

            _logger.LogDebug($"media exist");

            var featuredImageUrl = mediaDetail.FeaturedImage;
            var featuredImageFileName = mediaDetail.FeaturedImageMetadata;

            // get updated list of srtFiles
            var srtList = model.LstSrt != null ? JsonConvert.DeserializeObject<List<SrtFileModel>>(model.LstSrt) : null;
            if (srtList != null && srtList.Count > 0)
            {
                lstSrtUuids = mediaDetail.MediaSrt.Select(x => x.File).ToList();
                lstSrtFileNames = mediaDetail.MediaSrt.Select(x => x.FileMetadata).ToList();
                lstOriginalSrtUuids = mediaDetail.MediaSrt.Select(x => x.File).ToList();
                lstOriginalSrtFileNames = mediaDetail.MediaSrt.Select(x => x.FileMetadata).ToList();
            }
            if (model.Tags != null)
            {
                lstTags = model.Tags.Split(',');
            }

            //FeaturedImage
            if (model.FeaturedImage == null)
            {
                if (!string.IsNullOrEmpty(mediaDetail.FeaturedImage) && string.IsNullOrEmpty(model.ImageFileName))
                {
                    await _s3BucketService.RemoveImageAsync(mediaDetail.FeaturedImage);
                    featuredImageUrl = string.Empty;
                    featuredImageFileName = string.Empty;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(mediaDetail.FeaturedImage))
                {
                    await _s3BucketService.RemoveImageAsync(mediaDetail.FeaturedImage);
                }

                featuredImageUrl = await _s3BucketService.UploadFileByTypeToStorageAsync(model.FeaturedImage, model.Id, EntityType.Media, FileTypeEnum.FeaturedImage.ToString());
                featuredImageFileName = model.FeaturedImage.FileName;
            }

            // add here for srt files
            if (srtList != null && srtList.Count > 0)
            {
                foreach (var item in srtList)
                {
                    //if  base64 not null then new file upload on S3 
                    if (!string.IsNullOrEmpty(item.SrtFile))
                    {
                        var mediaSrtItem = new MediaSrtItem();
                        var file = Base64ToFormFile(item.SrtFile.Split("base64,")[1], item.SrtFileName); // SF-1261 Fixed for Safari (Mac Pro)
                        if (file != null)
                        {
                            file.Headers = new HeaderDictionary();
                            file.ContentType = "text/plain";
                            mediaSrtItem.File = await _s3BucketService.UploadFileToStorageAsync(file);
                        }

                        mediaSrtItem.FileMetaData = item.SrtFileName;
                        mediaSrtItem.Language = item.SrtLanguage;
                        mediaSrtItem.IsAdd = true;
                        mediaSrtItems.Add(mediaSrtItem);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.Uuid))
                        {
                            foreach (var srtDbItems in mediaDetail.MediaSrt )
                            {
                                srtList = srtList.Where(x => x.Uuid != null).ToList();
                                var isCount = srtList.Where(x => x.Uuid == srtDbItems.File).Count();
                                if (isCount == 0)//means does not exist in updated list then removed from s3 and DB as well
                                {
                                    await _s3BucketService.RemoveImageAsync(srtDbItems.File);
                                    var mediaSrtItem = new MediaSrtItem
                                    {
                                        File = item.SrtFileName,
                                        Language = item.SrtLanguage,
                                        IsAdd = false,
                                        Id = srtDbItems.Id
                                    };
                                    mediaSrtItems.Add(mediaSrtItem);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (mediaDetail.MediaSrt.Count > 0)
                {
                    foreach (var srtDbItems in mediaDetail.MediaSrt )
                    {
                        await _s3BucketService.RemoveImageAsync(srtDbItems.File);
                        var mediaSrtItem = new MediaSrtItem
                        {
                            IsAdd = false,
                            Id = srtDbItems.Id
                        };
                        mediaSrtItems.Add(mediaSrtItem);
                    }
                }
            }
            //end srt file add
            if (!string.IsNullOrEmpty(model.Url))
            {
                var mediaType = model.Url.Split('.').Last();
                string[] videoType = { "mov", "mp4", "m4v", "webm", "ogv", "mpg", "mpeg" };
                if (videoType.Contains(mediaType))
                {
                    model.Url = model.Url.Split('.').First() + ".mp4";
                    hlsUrl = $"{ model.Url.Split('.').First() }/hlsv3/index.m3u8";
                }
                else if (mediaType == VideoFileTypeEnum.m3u8.ToString())
                {
                    model.Url = mediaDetail.Url;
                    hlsUrl = mediaDetail.HlsUrl;
                }
            }
            var publishedDate = mediaDetail.DatePublishedUtc;
            var publishUserId = mediaDetail.PublishUserId;
            var seoUrl = mediaDetail.SeoUrl;
            var draftMediaSeoUrl = model.DraftMediaSeoUrl;

            // When media update from Draft to Publish
            if (model.MediaStatusId == (int)MediaStatusEnum.Published)
            {
                if (mediaDetail.MediastatusId == (int)MediaStatusEnum.Draft)
                {
                    publishedDate = DateTime.UtcNow;
                    publishUserId = userId;
                    seoUrl = !string.IsNullOrEmpty(draftMediaSeoUrl) ? draftMediaSeoUrl : Helper.SeoFriendlyUrl(model.Title);
                }
                if (mediaDetail.MediastatusId == (int)MediaStatusEnum.Published)
                {
                    draftMediaSeoUrl = mediaDetail.DraftMediaSeoUrl;
                }
            }

            media = await EditMedia(new Media
            {
                Id = model.Id,
                Name = model.Title,
                Description = model.Description ?? string.Empty ,
                LongDescription = model.LongDescription ,
                SeriesId = model.SeriesId,
                SourceId = model.SourceId,
                ActiveFromUtc = Helper.ConvertMomentToDateTime(model.ActiveFromUtc),
                ActiveToUtc = Helper.ConvertMomentToDateTime(model.ActiveToUtc),
                IsPrivate = model.IsPrivate,
                IsSharingAllowed = model.IsSharingAllowed,
                Url = model.Url ?? string.Empty ,
                HlsUrl = hlsUrl,
                Thumbnail = model.Thumbnail,
                DatePublishedUtc = publishedDate,
                DateLastupdatedUtc = DateTime.UtcNow,
                MediastatusId = model.MediaStatusId,
                EmbeddedCode = model.EmbeddedCode ?? string.Empty ,
                PublishUserId = publishUserId,
                FeaturedImage = featuredImageUrl ?? string.Empty ,
                FeaturedImageMetadata = featuredImageFileName,
                Metadata = model.MediaMetaData,
                SeoUrl = seoUrl,
                MediaAnnotation = lstNewMediAnnotations,
                DraftMediaSeoUrl = draftMediaSeoUrl,
                SrtFile = null,
                SrtFileMetadata = null,
                UniqueId = model.UniqueId,
                IsVisibleOnGoogle = model.IsVisibleOnGoogle
            }, lstTags, mediaSrtItems, lstTopicIds, model.ResourceIds, userId);

            var user = _context.User.Include(x => x.Usertype).Where(x => x.Id == userId).FirstOrDefault();
            if (!string.IsNullOrEmpty(user.Usertype.Name) && user.Usertype.Name == "Partner-User")
            {
                // Send Mail to Admin
                PartnerMediaReviewMail(user.Name, model.Id);
                _logger.LogDebug($"Send Mail to Admin");
            }

            if (media != null)
            {
                var mediaModel = new MediaShortModel
                {
                    Id = media.Id,
                    Name = media.Name,
                };

                // update on cloud
                _logger.LogDebug($"getting data from DB to cloud");

                // Get the data of media from db to update at cloud

                var mediaDetails = await _context.Media.Include(x => x.Mediastatus).Include(y => y.Mediatype).Include(z => z.Series)
                                   .Include(m => m.PublishUser).Include(n => n.UploadUser).Include(o => o.Source)
                                   .Where(p => p.Id == media.Id).FirstOrDefaultAsync();

                var topics = _context.Media.Include(x => x.MediaTopic).ThenInclude(y => y.Topic)
                                              .Where(z => z.Id == media.Id && z.MediaTopic.Count > 0)
                                              .SelectMany(x => x.MediaTopic , (entity , mediaTopic) => new
                                              {
                                                  Media = entity ,
                                                  mTopic = mediaTopic.Topic
                                              }).Select(x => x.mTopic.Name).ToList();

                var clousearchdentity = new MediaCloudSearchEntity
                {
                    Id = media.Id,
                    Title = media.Name,
                    Description = media.Description,
                    LongDescription = media.LongDescription,
                    SeriesTitle = mediaDetails.Series?.Name,
                    TopicTitle = topics,
                    Tags = lstTags == null ? new List<string>() : lstTags.ToList(),
                    Status = mediaDetails.Mediastatus?.Name,
                    MediaType = mediaDetails.Mediatype?.Name,
                    Date = mediaDetails.Mediastatus?.Name.ToLower() == "published" ? DateTime.Now.ToString() : "",
                    Source = mediaDetails.Source?.Name,
                    UploadedBy = mediaDetails.UploadUser?.Name,
                    PublishedBy = mediaDetails.PublishUser?.Name,
                    Logo = media.FeaturedImage,
                    IsPrivate = Convert.ToInt32(model.IsPrivate),
                    ActiveFrom = media.ActiveFromUtc,
                    ActiveTo = media.ActiveToUtc,
                    LastUpdatedDate = media.DateLastupdatedUtc,
                    IsSharingAllowed = Convert.ToInt32(media.IsSharingAllowed),
                    thumbnail = media.Thumbnail,
                    seourl = media.SeoUrl,
                    UniqueId = media.UniqueId,
                    IsDeleted = Convert.ToInt32(media.IsDeleted),
                    IsVisibleOnGoogle = Convert.ToInt32(media.IsVisibleOnGoogle)
                };
                var status = _cloudMediaSearchProvider.UpdateToCloud(clousearchdentity);

                if (status.ToLower().Trim() != CloudStatus.success.ToString())
                {
                    _logger.LogError($"Update:Received: request for Controller:MediaController and Action: EditMediaAsync Updated cloud status {status}");
                }
                return mediaModel;
            }

            _logger.LogDebug($"Update:Controller:MediaController and Action:EditMediaAsync unable to add media beacuse media is null");
            throw new BusinessException("Can not add a new media. Please, try again.");
        }

        public async Task UpdateAllMediaOnCloud()
        {
            var allMedia = (from media in _context.Media.Include(x => x.Mediatype).Include(y => y.Mediastatus).Include(z => z.Series)
                            .Include(m => m.Source).Include(n => n.UploadUser).Include(o => o.PublishUser)
                            let topicNames = (from x in _context.Media.Where(x => x.Id == media.Id)
                                              join y in _context.MediaTopic
                                              on x.Id equals y.MediaId
                                              join t in _context.Topic
                                              on y.TopicId equals t.Id
                                              select t.Name).ToList()
                            let tags = (from mtags in _context.MediaTag.Where(x => x.MediaId == media.Id)
                                        join tagsItem in _context.Tag
                                        on mtags.TagId equals tagsItem.Id
                                        select tagsItem.Name).ToList()
                            select new MediaCloudSearchEntity
                            {
                                Id = media.Id,
                                Title = media.Name,
                                Description = media.Description,
                                LongDescription = media.LongDescription,
                                SeriesTitle = media.Series.Name,
                                TopicTitle = topicNames,
                                Tags = tags,
                                Status = media.Mediastatus.Name,
                                MediaType = media.Mediatype.Name,
                                Date = media.DatePublishedUtc.ToString(),
                                Source = media.Source.Name,
                                UploadedBy = media.UploadUser.Name,
                                PublishedBy = media.PublishUser.Name,
                                Logo = media.FeaturedImage,
                                IsPrivate = Convert.ToInt32(media.IsPrivate),
                                ActiveFrom = media.ActiveFromUtc,
                                ActiveTo = media.ActiveToUtc,
                                LastUpdatedDate = media.DateLastupdatedUtc,
                                IsSharingAllowed = Convert.ToInt32(media.IsSharingAllowed),
                                thumbnail = media.Thumbnail,
                                seourl = media.SeoUrl,
                                IsDeleted = Convert.ToInt32(media.IsDeleted),
                                UniqueId = media.UniqueId,
                                IsVisibleOnGoogle = Convert.ToInt32(media.IsVisibleOnGoogle)
                            }).ToList<dynamic>();
            _cloudMediaSearchProvider.BulkUpdateToCloud(allMedia);
        }

        public async Task<PartnerMediaModel> SendToPartnerAsync(AddSendToPartnerModel model)
        {
            PartnerMedia partnerMedia;

            var mediaDetail = await _context.Media.Where(x => x.Id == model.MediaId).SingleOrDefaultAsync();
            if (mediaDetail is null)
            {
                _logger.LogDebug($"SendRequest:Controller:MediaController and action SendToPartnerAsync Media not exist");
                throw new BusinessException("Media not exist");
            }

            var partnerDetail = await _context.Partner.Where(x => x.Id == model.PartnerId).SingleOrDefaultAsync();
            if (partnerDetail is null)
            {
                _logger.LogDebug($"SendRequest:Controller:MediaController and action SendToPartnerAsync Partner not exist");
               throw new BusinessException("Partner not exist");
            }

            var startDate = Helper.ConvertMomentToDateTime(model.StartDate);
            if (startDate == null)
            {
                _logger.LogDebug($"SendRequest:Controller:MediaController and action SendToPartnerAsync Incorrect Start Date");
                throw new BusinessException("Incorrect Start Date");
            }

            var endDate = Helper.ConvertMomentToDateTime(model.EndDate);
            if (endDate == null)
            {
                _logger.LogDebug($"SendRequest:Controller:MediaController and action SendToPartnerAsync Incorrect End Date");
                throw new BusinessException("Incorrect End Date");
            }

            var objPartnerMedia = new PartnerMedia
            {
                PartnerId = model.PartnerId,
                MediaId = model.MediaId,
                Email = model.Email,
                StartDateUtc = startDate.Value,
                EndDateUtc = endDate.Value,
                IsExpired = false
            };

            var isExist = _context.PartnerMedia.Any(x => x.PartnerId == model.PartnerId && x.MediaId == model.MediaId);
            if (isExist)
            {
                partnerMedia = await EditPartnerMedia(objPartnerMedia);
            }
            else
            {
                partnerMedia = await AddPartnerMedia(objPartnerMedia);
            }
            // Send To Partner Mail

            SendToPartnerMail(model.Email, mediaDetail.FeaturedImage, mediaDetail.Thumbnail, model.Message, mediaDetail.Name, partnerDetail.Name,
                model.StartDate, model.EndDate, model.MediaId, mediaDetail.SeoUrl);
            _logger.LogDebug($"Controller:MediaController and action SendToPartnerAsync partner mail send");


            if (partnerMedia != null)
            {
                var partnerMediaModel = new PartnerMediaModel
                {
                    PartnerId = partnerMedia.PartnerId,
                    MediaId = partnerMedia.MediaId,
                    Email = partnerMedia.Email
                };
                return partnerMediaModel;
            }

            throw new BusinessException("Can not add a mediaPartner. Please, try again.");
        }

        public async Task DeleteMediaById(long id)
        {
            var media = await _context.Media.SingleOrDefaultAsync(t => t.Id == id);
            if (media == null)
                throw new BusinessException("Media not found");

            await SoftDeleteMedia(id);
            UpdateCloudMediaStatus(id);

            if (!string.IsNullOrEmpty(media.Url))
            {
                await _s3BucketService.RemoveImageAsync(media.Url);
            }

            if (!string.IsNullOrEmpty(media.FeaturedImage))
            {
                await _s3BucketService.RemoveImageAsync(media.FeaturedImage);
            }

            if (!string.IsNullOrEmpty(media.Thumbnail))
            {
                await _s3BucketService.RemoveImageAsync(media.Thumbnail);
            }
        }
        public async Task UpdateMediaUniqueIds(string fileName)
        {
            // Get Data from excel uploaded at S3 using fileName in function paramater
            var lstMedias = await _s3BucketService.ReadFromExcel(fileName);

          // fill that list from Excel file from S3
            foreach (var media in lstMedias)
            {
                var dbMedia = await _context.Media.SingleOrDefaultAsync(x => x.Id == media.Id && (x.UniqueId == null || x.UniqueId == ""));
                if (dbMedia != null)
                {
                    dbMedia.UniqueId = media.UniqueId;
                    _context.Update(dbMedia);
                    await _context.SaveChangesAsync();

                    // update uniqueId at cloud
                    await UpdateCloudMediaUniqueId(media.Id, media.UniqueId);
                }
            }
        }

        #region Private Methods
        private async Task<Media> SoftDeleteMedia(long mediaId)
        {
            var media = await _context.Media.SingleOrDefaultAsync(x => x.Id == mediaId);
            if (media != null)
            {

                media.IsDeleted = true;
                _context.Media.Update(media);
                await _context.SaveChangesAsync();
                return media;
            }
            return null;
        }
        private string UpdateCloudMediaStatus(long mediaId)
        {
            var response = string.Empty;
            var media = _cloudMediaSearchProvider.GetCurrentItemFromCloud(mediaId.ToString(), 10, 1).FirstOrDefault();
            if (media != null)
            {
                var clousearchdentity = new MediaCloudSearchEntity
                {
                    Id = Convert.ToInt64(media.id),
                    Title = media.title,
                    Description = media.description,
                    LongDescription = media.longdescription,
                    SeriesTitle = media.seriestitle,
                    TopicTitle = media.topictitle,
                    ToolTitle = media.tooltitle,
                    Tags = media.tags ?? new List<string>(),
                    Status = media.status,
                    MediaType = media.mediatype,
                    Date = media.date,
                    Source = media.source,
                    UploadedBy = media.uploadedby,
                    PublishedBy = media.publisedby,
                    Logo = media.logo,
                    IsPrivate = media.isprivate,
                    ActiveFrom = media.activefrom,
                    ActiveTo = media.activeto,
                    LastUpdatedDate = DateTime.UtcNow,
                    IsSharingAllowed = media.issharingallowed,
                    thumbnail = media.thumbnail,
                    seourl = media.seourl,
                    UniqueId = media.uniqueid,
                    IsDeleted = 1,
                    IsVisibleOnGoogle = media.isvisibleongoogle
                };
                var status = _cloudMediaSearchProvider.UpdateToCloud(clousearchdentity);
                if (status.ToLower().Trim() != CloudStatus.success.ToString())
                {
                    _logger.LogError($"Update:Controller:MediaController and Action:DeleteMediaById failure. {status}");

                }
            }
            return response;
        }
        private async Task<Media> AddMedia(Media entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        private async Task<BaseResponse> ArchiveMedia(int mediaId)
        {
            BaseResponse response;
            var media = _context.Media.SingleOrDefault(x => x.Id == mediaId);
            if (media != null)
            {
                //const string archiveMediaStatus = "Archived";
                media.MediastatusId = _context.MediaStatus.Where(st => st.Name == MediaStatusEnum.Archived.ToString()).Select(st => st.Id).SingleOrDefault();
                media.DateLastupdatedUtc = DateTime.UtcNow;
                _context.Media.Update(media);
                await _context.SaveChangesAsync();
                response = new BaseResponse() { ErrorDescription = "Success", ErrorCode = 200 };
            }
            else
            {
                response = new BaseResponse() { ErrorDescription = "Not Found", ErrorCode = 404 };
            }

            return response;
        }
        private async Task<BaseResponse> UnarchiveMedia(int mediaId)
        {
            BaseResponse response;
            var media = _context.Media.SingleOrDefault(x => x.Id == mediaId);
            if (media != null)
            {
                //const string archiveMediaStatus = "Draft"; // this is unArchived state
                media.MediastatusId = _context.MediaStatus.Where(st => st.Name == MediaStatusEnum.Draft.ToString()).Select(st => st.Id)
                    .SingleOrDefault();
                media.PublishUserId = null;
                media.DatePublishedUtc = null;
                media.DateLastupdatedUtc = DateTime.UtcNow;
                _context.Media.Update(media);
                await _context.SaveChangesAsync();
                response = new BaseResponse() { ErrorDescription = "Success", ErrorCode = 200 };
            }
            else
            {
                response = new BaseResponse() { ErrorDescription = "Not Found", ErrorCode = 404 };
            }

            return response;
        }
        private async Task<Media> EditMedia(Media entity, string[] lstTags, List<MediaSrtItem> _listOfSrtFiles, List<int> topicIds, string resourceIds, int updatedBy)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    int? seriesId = null;
                    var dbMedia = await _context.Media.Include(x=>x.MediaResourceOrder).Include(y=>y.ToolMedia).Include(z => z.MediaAnnotation).SingleOrDefaultAsync(t => t.Id == entity.Id);
                    if (dbMedia != null)
                    {
                        seriesId = dbMedia.SeriesId;
                        dbMedia.Name = entity.Name;
                        dbMedia.Description = entity.Description;
                        dbMedia.LongDescription = entity.LongDescription;
                        dbMedia.MediastatusId = entity.MediastatusId;
                        dbMedia.PublishUserId = entity.PublishUserId;
                        dbMedia.DatePublishedUtc = entity.DatePublishedUtc;
                        dbMedia.Url = entity.Url ?? string.Empty;
                        dbMedia.HlsUrl = entity.HlsUrl;
                        dbMedia.Thumbnail = string.IsNullOrEmpty(entity.Thumbnail) ? dbMedia.Thumbnail : entity.Thumbnail ;
                        dbMedia.FeaturedImage = entity.FeaturedImage ?? string.Empty;
                        dbMedia.ActiveFromUtc = entity.ActiveFromUtc;
                        dbMedia.ActiveToUtc = entity.ActiveToUtc;
                        dbMedia.IsPrivate = entity.IsPrivate;
                        dbMedia.EmbeddedCode = entity.EmbeddedCode ?? string.Empty;
                        dbMedia.SeriesId = entity.SeriesId;
                        dbMedia.SourceId = entity.SourceId;
                        dbMedia.Metadata = entity.Metadata;
                        dbMedia.IsSharingAllowed = entity.IsSharingAllowed;
                        dbMedia.FeaturedImageMetadata = entity.FeaturedImageMetadata;
                        dbMedia.SrtFile = entity.SrtFile ?? string.Empty;
                        dbMedia.SrtFileMetadata = entity.SrtFileMetadata ?? string.Empty;
                        dbMedia.SeoUrl = entity.SeoUrl;
                        dbMedia.DraftMediaSeoUrl = entity.DraftMediaSeoUrl;
                        dbMedia.DateLastupdatedUtc = entity.DateLastupdatedUtc;
                        dbMedia.UniqueId = entity.UniqueId;
                        dbMedia.IsVisibleOnGoogle = entity.IsVisibleOnGoogle;
                        if (entity.SeriesId != null)
                        {
                            dbMedia.SeriesId = entity.SeriesId;
                        }
                        //if (entity.TopicId != null)
                        //{
                        //    dbMedia.TopicId = entity.TopicId;
                        //}
                        if (entity.SourceId != null)
                        {
                            dbMedia.SourceId = entity.SourceId;
                        }
                        _context.Media.Update(dbMedia);

                        //Add Media Srt 
                        if (_listOfSrtFiles != null)
                        {
                            var lstNewSrt = new List<MediaSrt>();
                            var lstNewSrtRemove = new List<MediaSrt>();

                            foreach (var item in _listOfSrtFiles)
                            {
                                // var isExist = _context.MediaSrt.Where(x => x.File.ToLower().Trim() == item.File.ToLower().Trim() && x.MediaId==entity.Id).Any();
                                if (item.IsAdd)
                                {
                                    lstNewSrt.Add(new MediaSrt { File = item.File, FileMetadata = item.FileMetaData, Language = item.Language, MediaId = entity.Id });
                                }
                                else
                                {
                                    var lstOldSrtFiles = await _context.MediaSrt.Where(x => x.Id == item.Id).ToListAsync();
                                    _context.RemoveRange(lstOldSrtFiles);
                                }
                            }
                            // add in media srt when not existin media srt
                            if (lstNewSrt.Count > 0)
                            {
                                _context.MediaSrt.AddRange(lstNewSrt);
                                await _context.SaveChangesAsync();
                            }
                        }
                        // Add multiple topic for single media
                        // var mediaTopic= _context.MediaTopic

                        if (topicIds != null)
                        {
                            var mediaTopic = _context.MediaTopic.Where(x => x.MediaId == entity.Id).Select(x => x.TopicId).ToList();

                            var lstNewTopic = new List<MediaTopic>();
                            //  var result = mediaTopic.Except(topicIds);

                            foreach (var topicItem in topicIds)
                            {
                                if (!mediaTopic.Contains(topicItem))
                                {
                                    lstNewTopic.Add(new MediaTopic { MediaId = entity.Id, TopicId = topicItem });

                                }
                            }
                            // Add new Tags
                            if (lstNewTopic.Count <= 0)
                            {
                            }
                            else
                            {
                                _context.MediaTopic.AddRange(lstNewTopic);
                                await _context.SaveChangesAsync();
                            }
                            // Remove Topic if not in updated List

                            foreach (var dbTopicItem in mediaTopic)
                            {
                                if (!topicIds.Contains(Convert.ToInt32(dbTopicItem)))
                                {
                                    var lstOldTopic = await _context.MediaTopic.Where(x => x.MediaId == entity.Id && x.TopicId == dbTopicItem).ToListAsync();
                                    _context.RemoveRange(lstOldTopic);
                                }
                            }
                        }

                        // Add Tags
                        if (lstTags != null)
                        {
                            var lstNewTags = new List<Tag>();
                            foreach (var item in lstTags)
                            {
                                var tag = item.Trim();
                                var isTagExist = _context.Tag.Where(x => x.Name == tag).Any();
                                if (!isTagExist)
                                {
                                    lstNewTags.Add(new Tag { Name = tag });
                                }
                            }
                            if (lstNewTags.Count > 0)
                            {
                                _context.Tag.AddRange(lstNewTags);
                                await _context.SaveChangesAsync();
                            }
                        }
                        var lstOldTags = await _context.MediaTag.Where(x => x.MediaId == entity.Id).ToListAsync();
                        _context.RemoveRange(lstOldTags);
                        var lstMediaTags = new List<MediaTag>();
                        if (lstTags != null)
                        {
                            foreach (var item in lstTags)
                            {
                                var tag = item.Trim();
                                var tagId = await _context.Tag.Where(x => x.Name == tag).Select(x => x.Id).SingleOrDefaultAsync();
                                lstMediaTags.Add(new MediaTag { MediaId = entity.Id, TagId = tagId });
                            }
                        }
                        if (lstMediaTags.Count > 0)
                        {
                            _context.MediaTag.AddRange(lstMediaTags);
                        }

                        // Add/Update Media Resource Order


                        #region // manage the resourceIds after select or deselect the series from the media  

                        //var combineResourceIds = resourceIds!= null ? resourceIds.Trim().Split(",").ToList().ConvertAll(int.Parse) : new List<int>();
                        
                        var mos = 0;
                        var combineResourceIds = resourceIds!= null ? resourceIds.Trim().Split(",").Where(m => int.TryParse(m , out mos)).Select(m => int.Parse(m)).ToList() : new List<int>();
                        if ( seriesId != null && entity.SeriesId == null )
                        {
                            #region Remove the reference of resourceIds after deselect the series  
                            var removableResourceId = _context.ToolSeries.Where(x => x.SeriesId == seriesId.Value).Select(y => y.ToolId).ToList();

                            // get the same resourceId attached to particular media
                            var mediaResourceIds = dbMedia.ToolMedia.Select(x => x.ToolId).ToList();
                            removableResourceId = removableResourceId.Except(mediaResourceIds).ToList();

                            if ( removableResourceId.Count > 0 )
                            {
                                combineResourceIds = combineResourceIds.Except(removableResourceId).ToList();
                            }
                            #endregion
                        }
                        else if ( seriesId == null && entity.SeriesId != null )
                        {
                            #region // Add the refrence resourceIds after select the series  
                            var newSeriesResourceId = _context.ToolSeries.Where(x => x.SeriesId == entity.SeriesId.Value).Select(y => y.ToolId).ToList();

                            if ( newSeriesResourceId.Count > 0 )
                            {
                                combineResourceIds = combineResourceIds.Union(newSeriesResourceId).ToList();
                            }
                            #endregion
                        }
                        else if ( seriesId != null && entity.SeriesId != null )
                        {
                            if ( seriesId != entity.SeriesId )
                            {
                                #region // Remove the resourceIds from the old Series 
                                var removableResourceId = _context.ToolSeries.Where(x => x.SeriesId == seriesId.Value).Select(y => y.ToolId).ToList();

                                // get the same resourceId attached to particular media
                                var mediaResourceIds = dbMedia.ToolMedia.Select(x => x.ToolId).ToList();
                                removableResourceId = removableResourceId.Except(mediaResourceIds).ToList();

                                if ( removableResourceId.Count > 0 )
                                {
                                    combineResourceIds = combineResourceIds.Except(removableResourceId).ToList();
                                }
                                #endregion

                                #region // Add the resourceIds for the new series
                                var newSeriesResourceId = _context.ToolSeries.Where(x => x.SeriesId == entity.SeriesId.Value).Select(y => y.ToolId).ToList();

                                if ( newSeriesResourceId.Count > 0 )
                                {
                                    combineResourceIds = combineResourceIds.Union(newSeriesResourceId).ToList();
                                }
                            }
                            #endregion
                        }

                        #endregion
                        if ( combineResourceIds.Count > 0)
                        {
                            var updatedResourceIds = string.Join("," , combineResourceIds);
                            if ( dbMedia.MediaResourceOrder.Count > 0)
                            {
                                var mediaResource = dbMedia.MediaResourceOrder.FirstOrDefault();
                                if(mediaResource != null)
                                {
                                    mediaResource.ResourceIds = updatedResourceIds;
                                    mediaResource.UpdatedAtUtc = DateTime.UtcNow;
                                    mediaResource.UpdatedBy = updatedBy;
                                }
                                _context.MediaResourceOrder.Update(mediaResource);
                            }
                            else
                            {
                                var newMediaResource = new MediaResourceOrder
                                {
                                    MediaId = entity.Id ,
                                    ResourceIds = updatedResourceIds ,
                                    UpdatedAtUtc = DateTime.UtcNow,
                                    UpdatedBy = updatedBy
                                };
                                await _context.MediaResourceOrder.AddAsync(newMediaResource);
                            }
                        }
                        else
                        {
                            var mediaResource = dbMedia.MediaResourceOrder.FirstOrDefault();
                            if(mediaResource != null)
                            {
                                _context.MediaResourceOrder.Remove(mediaResource);
                            }
                        }

                        // Add/Update the Media Annotation
                        
                        if(dbMedia.MediaAnnotation.Count > 0)
                        {
                             _context.MediaAnnotation.RemoveRange(dbMedia.MediaAnnotation);
                        }

                        if(entity.MediaAnnotation.Count > 0)
                        {
                            await _context.MediaAnnotation.AddRangeAsync(entity.MediaAnnotation);
                        }
                       
                        await _context.SaveChangesAsync();
                        transaction.Commit();
                        return dbMedia;
                    }
                }
                catch
                {
                    transaction.Rollback();
                }
            }
            return null;
        }
        private async Task<PartnerMedia> AddPartnerMedia(PartnerMedia entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }
        private async Task<PartnerMedia> EditPartnerMedia(PartnerMedia entity)
        {
            var dbpartnerMedia = await _context.PartnerMedia.SingleOrDefaultAsync(partnerMedia => partnerMedia.PartnerId == entity.PartnerId && partnerMedia.MediaId == entity.MediaId);
            if (dbpartnerMedia != null)
            {
                dbpartnerMedia.Email = entity.Email;
                dbpartnerMedia.StartDateUtc = entity.StartDateUtc;
                dbpartnerMedia.EndDateUtc = entity.EndDateUtc;
                _context.PartnerMedia.Update(dbpartnerMedia);
                await _context.SaveChangesAsync();
                return dbpartnerMedia;
            }
            return null;
        }
        private string UpdateCloudMedia(long mediaId)
        {
            var dbMedia = (from media in _context.Media.Include(x => x.Mediatype).Include(x => x.Mediastatus).Include(x => x.UploadUser)
                           .Include(x => x.Series).Include(x => x.Source).Include(x => x.PublishUser).Where(x => x.Id == mediaId).ToList()
                           let topicNames = (from x in _context.Media.Where(x => x.Id == media.Id)
                                             join y in _context.MediaTopic
                                             on x.Id equals y.MediaId
                                             join t in _context.Topic
                                             on y.TopicId equals t.Id
                                             select t.Name).ToList()
                           let tags = (from mtags in _context.MediaTag.Where(x => x.MediaId == media.Id)
                                       join tagsItem in _context.Tag
                                       on mtags.TagId equals tagsItem.Id
                                       select tagsItem.Name).ToList()
                           select new MediaCloudSearchEntity
                           {
                               Id = media.Id,
                               Title = media.Name,
                               Description = media.Description,
                               LongDescription = media.LongDescription,
                               SeriesTitle = media.Series?.Name,
                               TopicTitle = topicNames,
                               Tags = tags,
                               Status = media.Mediastatus?.Name,
                               MediaType = media.Mediatype?.Name,
                               Date = media.Mediastatus?.Name.ToString().ToLower() == "published" ? media.DatePublishedUtc.ToString() : "",
                               Source = media.Source?.Name,
                               UploadedBy = media.UploadUser?.Name,
                               PublishedBy = media.PublishUser?.Name,
                               Logo = media.FeaturedImage,
                               IsPrivate = Convert.ToInt32(media.IsPrivate),
                               ActiveFrom = media.ActiveFromUtc,
                               ActiveTo = media.ActiveToUtc,
                               IsSharingAllowed = Convert.ToInt32(media.IsSharingAllowed),
                               thumbnail = media.Thumbnail,
                               seourl = media.SeoUrl,
                               LastUpdatedDate = media.DateLastupdatedUtc,
                               UniqueId = media.UniqueId ,
                               IsDeleted = Convert.ToInt32(media.IsDeleted) ,
                               IsVisibleOnGoogle = Convert.ToInt32(media.IsVisibleOnGoogle)
                           } ).FirstOrDefault();
            return _cloudMediaSearchProvider.UpdateToCloud(dbMedia);
        }
        private async Task<Media> UpdateMedia(long mediaId, string newUrl)
        {
            var media = await _context.Media.SingleOrDefaultAsync(x => x.Id == mediaId);
            if (media != null)
            {
                var isUrlExist = await _context.Media.Where(x => x.SeoUrl.Trim() == newUrl).AnyAsync();
                if (isUrlExist)
                {
                    newUrl = $"{newUrl}-1";
                }
                media.SeoUrl = newUrl.Trim();
                _context.Media.Update(media);
                await _context.SaveChangesAsync();
                return media;
            }
            return null;
        }
        private async void PartnerMediaReviewMail(string partnerName, long mediaId)
        {
            var callbackUrl = $"{EnvironmentVariables.AdminUiUrl}/edit-media/{mediaId}";
            var companyLogo = $"{EnvironmentVariables.ClientUiUrl}/ms_logo.png"; //await S3Utility.RetrieveImageWithSignedUrl("ms_logo.png"); // Need to replace company logo uuid when change
            var body = EmailTemplates.Templates[Templates.PartnerUserMedia]
               .Replace("{{partnerName}}", partnerName)
               .Replace("{{companyLogo}}", companyLogo)
               .Replace("{{resetLink}}", callbackUrl);

            // Mail to Million Stories
            var emailAgency = new SendEmailBuilder(EnvironmentVariables.EmailQueue)
                .From(_emailSettings.From.noreply)
                .To(_emailSettings.From.info)
                .Subject(_emailSettings.Subject.MediaReviewSubmission)
                .Action(EmailActions.PartnerUserMedia)
                .Body(body)
                .Build();

            await _event.Publish(emailAgency);
        }
        private FormFile Base64ToFormFile(string base64String, string fileName)
        {
            var imageBytes = Convert.FromBase64String(base64String);
            var stream = new MemoryStream(imageBytes, 0, imageBytes.Length);
            var file = new FormFile(stream, 0, stream.Length, null, fileName);
            return file;
        }
        private async void SendToPartnerMail(string receiverEmail, string featuredImage, string thumbnail, string message, string mediaTitle, string partnerName, string activeFrom, string activeTo, long mediaId, string seoUrl)
        {
            var embedSource = $"{EnvironmentVariables.ClientUiUrl}/media/{seoUrl}?video={mediaId}&sendToPartner=true&embedded=true"; //$"{AppSettings.ClientUiUrl}/media?v={encMediaId}&p={encPartnerId}";
            string imageSource;
            if (!string.IsNullOrEmpty(featuredImage))
            {
                imageSource = !string.IsNullOrEmpty(featuredImage) ? _s3BucketService.RetrieveImageCDNUrl(featuredImage) : string.Empty;
            }
            else
            {
                imageSource = !string.IsNullOrEmpty(thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(thumbnail) : string.Empty;
            }
            var companyLogo = $"{EnvironmentVariables.ClientUiUrl}/ms_logo.png";//await S3Utility.RetrieveImageWithSignedUrl("ms_logo.png"); TODO: Need to replace company logo uuid when change

            if (DateTime.TryParse(activeFrom, out var dateFrom))
            {
                activeFrom = dateFrom.ToString();
            }
            if (DateTime.TryParse(activeTo, out var dateTo))
            {
                activeTo = dateTo.ToString();
            }

            var embeddedCode = @"<iframe src = '{{embedSource}}' 
                                   width = '100%' 
                                   height = '100%' 
                                   frameborder = '0' 
                                   allowfullscreen = ''
                                   allow='encrypted-media'
                                   style = 'position:absolute; top:0; left: 0' />";

            embeddedCode = embeddedCode.Replace("{{embedSource}}", embedSource);
            // Send Mail Using SES
            var body = EmailTemplates.Templates[Templates.SendToPartner]
                        .Replace("{{partnerName}}", partnerName)
                        .Replace("{{mediaTitle}}", mediaTitle)
                        .Replace("{{activeFrom}}", activeFrom)
                        .Replace("{{activeTo}}", activeTo)
                        .Replace("{{embeddedCode}}", embeddedCode)
                        .Replace("{{message}}", message)
                        .Replace("{{imageSource}}", imageSource)
                        .Replace("{{companyLogo}}", companyLogo);

            _logger.LogDebug($"Email Body: {body}");

            var email = new SendEmailBuilder(EnvironmentVariables.EmailQueue)
                .From(_emailSettings.From.noreply)
                .To(receiverEmail)
                .Subject(_emailSettings.Subject.SendToPartner)
                .Action(EmailActions.SendToPartner)
                .Body(body)
                .Build();

            await _event.Publish(email);
            _logger.LogDebug($"Email: {email}");
        }

        private async Task UpdateCloudMediaUniqueId(long mediaId, string uniqueId)
        {
            var media = _cloudMediaSearchProvider.GetCurrentItemFromCloud(mediaId.ToString(), 10, 1).FirstOrDefault();
            if (media != null)
            {
                var clousearchdentity = new MediaCloudSearchEntity
                {
                    Id = Convert.ToInt64(media.id),
                    Title = media.title,
                    Description = media.description,
                    LongDescription = media.longdescription,
                    SeriesTitle = media.seriestitle,
                    TopicTitle = media.topictitle,
                    ToolTitle = media.tooltitle,
                    Tags = media.tags ?? new List<string>(),
                    Status = media.status,
                    MediaType = media.mediatype,
                    Date = media.date,
                    Source = media.source,
                    UploadedBy = media.uploadedby,
                    PublishedBy = media.publisedby,
                    Logo = media.logo,
                    IsPrivate = media.isprivate,
                    ActiveFrom = media.activefrom,
                    ActiveTo = media.activeto,
                    LastUpdatedDate = media.lastupdateddate,
                    IsSharingAllowed = media.issharingallowed,
                    thumbnail = media.thumbnail,
                    seourl = media.seourl,
                    UniqueId = uniqueId,
                    IsDeleted = media.isdeleted,
                    IsVisibleOnGoogle = media.isvisibleongoogle
                };
                var status = _cloudMediaSearchProvider.UpdateToCloud(clousearchdentity);
                if (status.ToLower().Trim() != CloudStatus.success.ToString())
                {
                    _logger.LogError($"Update:Controller:MediaController and Action:UpdateMediaUniqueIds failure. {status}");

                }
            }
        }

        private bool IsTimespanOverlap(List<MediaAnnotationModel> mediAnnotations)
        {
            var intervals = new List<Interval>();

            foreach ( var item in mediAnnotations )
            {
                if (TimeSpan.TryParse(item.TimeStamp, out var startAt))
                {
                    intervals.Add(new Interval { start = startAt, end = startAt.Add(TimeSpan.FromSeconds(item.Duration)) });
                }
                else
                {
                    throw new BusinessException("Error creating media - Timespan is not in correct format");
                }
            };

            for ( var i = 1 ;i < intervals.Count ;i++ )
            {
                var overlap = intervals[i-1].start < intervals[i].end  && intervals[i].start  <= intervals[i-1].end;
                if ( overlap )
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }

    public class Interval
    {
        public TimeSpan start { get; set; }
        public TimeSpan end { get; set; }
      
    }
}
