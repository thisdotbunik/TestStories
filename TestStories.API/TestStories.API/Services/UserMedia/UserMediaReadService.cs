using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TestStories.API.Models.ResponseModels;
using TestStories.Common;
using TestStories.Common.Configurations;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;

namespace TestStories.API.Services
{
    public class UserMediaReadService : IUserMediaReadService
    {
        readonly TestStoriesContext _context;
        readonly IS3BucketService _s3BucketService;
        readonly AppSettings _appSettings;

        public UserMediaReadService (TestStoriesContext context , IOptions<AppSettings> appSettings, IS3BucketService s3BucketService)
        {
            _context = context;
            _appSettings = appSettings.Value;
            _s3BucketService = s3BucketService;
        }
        public async Task<MediaViewModel> MediaDetailAsync (int id, string userRole)
        {
            var dbMedia = await _context.Media.Include(x => x.ToolMedia).Include(y => y.MediaSrt).Where(z => z.Id == id).FirstOrDefaultAsync();
            if ( dbMedia != null && !dbMedia.IsDeleted )
            {
                if ( (MediaStatusEnum)dbMedia.MediastatusId != MediaStatusEnum.Published )
                {
                    if ( IsMediaActive(id) && userRole != "Admin" && userRole != "Admin-Editor" && userRole != "SuperAdmin" )
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
            if ( dbMedia.ToolMedia.Count > 0 )
            {
                var rand = new Random();
                var toSkip = rand.Next(0 , dbMedia.ToolMedia.Count);
                var toolMedia = dbMedia.ToolMedia.Skip(toSkip).Take(1).First();
                if ( toolMedia != null )
                {
                    toolDetail = new ShortToolModel();
                    toolDetail = ( from item in _context.Tool
                                   where item.Id == toolMedia.ToolId
                                   select new ShortToolModel
                                   {
                                       Id = item.Id ,
                                       ToolName = item.Name ,
                                       Url = item.Url ,
                                       Description = item.Description ,
                                       FeaturedImage = item.FeaturedImage ,
                                       ShowOnMenu = item.ShowOnMenu ,
                                       ShowOnHomePage = item.ShowOnHomepage
                                   } ).FirstOrDefault();
                }
            }
            else
            {
                if ( dbMedia.SeriesId != null )
                {
                    var lstToolSeries = await _context.ToolSeries.Where(x => x.SeriesId == dbMedia.SeriesId).ToListAsync();
                    if ( lstToolSeries.Count > 0 )
                    {
                        var rand = new Random();
                        var toSkip = rand.Next(0 , lstToolSeries.Count);
                        var toolSeries = lstToolSeries.Skip(toSkip).Take(1).First();
                        if ( toolSeries != null )
                        {
                            toolDetail = new ShortToolModel();
                            toolDetail = await ( from item in _context.Tool
                                                 where item.Id == toolSeries.ToolId
                                                 select new ShortToolModel
                                                 {
                                                     Id = item.Id ,
                                                     ToolName = item.Name ,
                                                     Url = item.Url ,
                                                     Description = item.Description ,
                                                     FeaturedImage = item.FeaturedImage ,
                                                     ShowOnMenu = item.ShowOnMenu ,
                                                     ShowOnHomePage = item.ShowOnHomepage
                                                 } ).FirstOrDefaultAsync();
                        }
                    }
                }
            }

            if ( toolDetail != null )
            {
                var featuredImage = toolDetail.FeaturedImage;
                toolDetail.FeaturedImage = !string.IsNullOrEmpty(featuredImage) ? _s3BucketService.RetrieveImageCDNUrl(featuredImage) : string.Empty;
                toolDetail.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage , EntityType.Tools);
            }

            tools = await ( from tool_Media in _context.ToolMedia
                            join tool in _context.Tool on tool_Media.ToolId equals tool.Id
                            where tool_Media.MediaId == id
                            select new ShortToolModel
                            {
                                Id = tool.Id ,
                                ToolName = tool.Name ,
                                Url = tool.Url ,
                                Description = tool.Description ,
                                FeaturedImage = tool.FeaturedImage ,
                                ShowOnMenu = tool.ShowOnMenu ,
                                ShowOnHomePage = tool.ShowOnHomepage
                            } )
                      .Union(from tool_Series in _context.ToolSeries
                             join media in _context.Media on tool_Series.SeriesId equals media.SeriesId
                             join tool in _context.Tool on tool_Series.ToolId equals tool.Id
                             where media.Id == id
                             select new ShortToolModel
                             {
                                 Id = tool.Id ,
                                 ToolName = tool.Name ,
                                 Url = tool.Url ,
                                 Description = tool.Description ,
                                 FeaturedImage = tool.FeaturedImage ,
                                 ShowOnMenu = tool.ShowOnMenu ,
                                 ShowOnHomePage = tool.ShowOnHomepage
                             }).ToListAsync();

            tools = tools.GroupBy(p => p.Id).Select(grp => grp.FirstOrDefault()).ToList();
            foreach ( var tool in tools )
            {
                var featuredImage = tool.FeaturedImage;
                tool.FeaturedImage = !string.IsNullOrEmpty(featuredImage) ? _s3BucketService.RetrieveImageCDNUrl(featuredImage) : string.Empty;
                tool.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage , EntityType.Tools);
            }

            var mediaTools = tools.Select(x => x.ToolName).ToList();
            var result = await ( from md in _context.Media.Include(x => x.Mediastatus).Include(y => y.Mediatype)
                                 .Include(z => z.PublishUser).Include(m => m.UploadUser)
                                 join sr in _context.Series on md.SeriesId equals sr.Id into srGroup
                                 from series in srGroup.DefaultIfEmpty()
                                 join prt in _context.Partner on md.SourceId equals prt.Id into prtGroup
                                 from partner in prtGroup.DefaultIfEmpty()
                                 where md.Id == id
                                 let tags = _context.MediaTag.Include(x => x.Tag).Where(y => y.MediaId == id).Select(x => x.Tag.Name).ToList()
                                 let topics = _context.Media.Include(x => x.MediaTopic).ThenInclude(y => y.Topic)
                                              .Where(z => z.Id == id && z.MediaTopic.Count > 0)
                                              .SelectMany(x => x.MediaTopic , (entity , mediaTopic) => new
                                              {
                                                  Media = entity ,
                                                  mTopic = mediaTopic.Topic
                                              }).Select(x => x.mTopic.Name).ToList()
                                 select new MediaViewModel
                                 {
                                     Id = md.Id ,
                                     Name = md.Name ,
                                     Description = md.Description ,
                                     LongDescription = md.LongDescription ,
                                     Url = _appSettings.IsHlsFormatEnabled == true && md.MediatypeId == 1 ? md.HlsUrl : md.Url ,
                                     EmbeddedCode = md.EmbeddedCode ,
                                     Logo = md.FeaturedImage ,
                                     ImageFileName = md.FeaturedImageMetadata ,
                                     Thumbnail = md.Thumbnail ,
                                     FeaturedImage = md.FeaturedImage ,
                                     MediaType = md.Mediatype.Name ,
                                     MediaStatus = md.Mediastatus.Name ,
                                     PublishDate = md.DatePublishedUtc ,
                                     CreatedDate = md.DateCreatedUtc ,
                                     Series = series.Name ,
                                     SeriesId = series != null ? series.Id : 0 ,
                                     Topic = topics.ToList() ,
                                     Source = partner.Name ,
                                     SourceId = partner != null ? partner.Id : 0 ,
                                     Tags = tags.ToList() ,
                                     IsPrivate = md.IsPrivate ,
                                     IsSharingAllowed = (bool)md.IsSharingAllowed ,
                                     ActiveFromUtc = md.ActiveFromUtc ,
                                     ActiveToUtc = md.ActiveToUtc ,
                                     MediaTypeId = md.MediatypeId ,
                                     MediaMetaData = md.Metadata ,
                                     PublishedById = md.PublishUserId ,
                                     PublishedBy = md.PublishUser.Name ,
                                     UploadedById = md.UploadUserId ,
                                     UploadedByUser = md.UploadUser.Name ,
                                     SrtFile = md.SrtFile ,
                                     SrtFileName = md.SrtFileMetadata ,
                                     SeoUrl = md.SeoUrl ,
                                     DraftMediaSeoUrl = md.DraftMediaSeoUrl ,
                                     LastUpdatedDate = md.DateLastupdatedUtc ,
                                     IsVisibleOnGoogle = md.IsVisibleOnGoogle ,
                                     UniqueId = md.UniqueId
                                 } ).SingleOrDefaultAsync();

            if ( result != null )
            {
                result.TopicIds = await _context.MediaTopic.Where(x => x.MediaId == id).Select(x => x.TopicId).ToListAsync();
                result.Tool = toolDetail;
                result.Tools = tools;
                result.MediaTools = mediaTools;

                var logo = result.Logo;
                result.Logo = !string.IsNullOrEmpty(logo) ? _s3BucketService.RetrieveImageCDNUrl(logo) : string.Empty;
                result.Logos = await _s3BucketService.GetCompressedImages(logo , EntityType.Media);

                var thumbnail = result.Thumbnail;
                result.Thumbnail = !string.IsNullOrEmpty(thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(thumbnail) : string.Empty;
                result.Thumbnails = await _s3BucketService.GetCompressedImages(thumbnail , EntityType.Media);

                var featuredImage = result.FeaturedImage;
                result.FeaturedImage = !string.IsNullOrEmpty(featuredImage) ? _s3BucketService.RetrieveImageCDNUrl(featuredImage) : string.Empty;
                result.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage , EntityType.Media);

                // get srtFile
                if (dbMedia.MediaSrt.Count > 0 )
                {
                    var lstSrt = new List<SrtFileModel>();
                    foreach ( var item in dbMedia.MediaSrt )
                    {
                        var srtFileDetail = new SrtFileModel
                        {
                            SrtFile = "" ,//  SrtFile = item != null ? await S3Utility.RetrieveImageWithSignedUrl(item.File, true) : string.Empty,
                            SrtFileName = item.FileMetadata ,
                            Uuid = item.File ,
                            SrtLanguage = item.Language ,
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
        public async Task<CollectionModel<UserWatchHistoryViewModel>> WatchHistoryAsync (int _userId)
        {
            if ( _userId != 0 )
            {
                var isUserExist = _context.User.Any(x => x.Id == _userId);
                if ( !isUserExist )
                    throw new BusinessException("User not exist");
            }
            var result = ( from watch in _context.WatchHistory.Include(x => x.Media).ThenInclude(y => y.Series).OrderByDescending(x => x.LastWatchedUtc)
                           where watch.UserId == _userId && watch.Media.MediastatusId != (int)MediaStatusEnum.Archived 
                           && !watch.Media.IsDeleted
                           && ( watch.Media.ActiveFromUtc.HasValue &&  watch.Media.ActiveToUtc.HasValue ? watch.Media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= watch.Media.ActiveToUtc :
                                          watch.Media.ActiveFromUtc.HasValue && !watch.Media.ActiveToUtc.HasValue ? watch.Media.ActiveFromUtc <= DateTime.UtcNow :
                                         watch.Media.ActiveFromUtc.HasValue || !watch.Media.ActiveToUtc.HasValue || DateTime.UtcNow <= watch.Media.ActiveToUtc )
                           select new UserWatchHistoryViewModel
                           {
                               Id = watch.Id ,
                               MediaId = watch.Media.Id ,
                               MediaName = watch.Media.Name ,
                               Description = watch.Media.Description ,
                               Thumbnail = watch.Media.Thumbnail ,
                               FeaturedImage = watch.Media.FeaturedImage ,
                               Url = watch.Media.Url ,
                               MediaTypeId = watch.Media.MediatypeId ,
                               IsSharingAllowed = watch.Media.IsSharingAllowed ,
                               SeoUrl = watch.Media.SeoUrl ,
                               UniqueId = watch.Media.UniqueId ,
                               SeriesId = watch.Media.SeriesId,
                               Series = watch.Media.Series.Name,
                           } ).ToList();
            foreach ( var watchHistory in result )
            {
                var featuredImage = watchHistory.FeaturedImage;
                watchHistory.FeaturedImage = !string.IsNullOrEmpty(watchHistory.FeaturedImage) ? _s3BucketService.RetrieveImageCDNUrl(watchHistory.FeaturedImage) : string.Empty;
                watchHistory.FeaturedImages = await _s3BucketService.GetCompressedImages(featuredImage , EntityType.Media);

                var thumbnail = watchHistory.Thumbnail;
                watchHistory.Thumbnail = !string.IsNullOrEmpty(watchHistory.Thumbnail) ? _s3BucketService.RetrieveImageCDNUrl(watchHistory.Thumbnail) : string.Empty;
                watchHistory.Thumbnails = await _s3BucketService.GetCompressedImages(thumbnail , EntityType.Media);
            }

            if ( result != null )
            {
                return new CollectionModel<UserWatchHistoryViewModel>
                {
                    Items = result ,
                    TotalCount = result.Count
                };
            }

            return new CollectionModel<UserWatchHistoryViewModel>();
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
        private bool IsMediaActive (long mediaId)
        {
            var media = _context.Media.FirstOrDefault(x => x.Id == mediaId);

            if ( media != null )
            {
                if ( media.ActiveFromUtc != null && media.ActiveToUtc != null )
                    return media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= media.ActiveToUtc;
                else if ( media.ActiveFromUtc != null && media.ActiveToUtc == null )
                    return media.ActiveFromUtc <= DateTime.UtcNow;
                else if ( media.ActiveFromUtc == null && media.ActiveToUtc != null )
                    return DateTime.UtcNow <= media.ActiveToUtc;
                else
                    return true;
            }
            return false;
        }
        private async Task<List<MediaInfoModel>> GetMediaPlayListAsync(int playlistId)
        {

            var result = (from plmedia in _context.PlaylistMedia.Include(x => x.Media)
                          where plmedia.PlaylistId == playlistId && plmedia.Media.MediastatusId != (int)MediaStatusEnum.Archived
                          && plmedia.Media.MediastatusId != (int)MediaStatusEnum.Draft && !plmedia.Media.IsDeleted
                          && ( plmedia.Media.ActiveFromUtc.HasValue &&  plmedia.Media.ActiveToUtc.HasValue ? plmedia.Media.ActiveFromUtc <= DateTime.UtcNow && DateTime.UtcNow <= plmedia.Media.ActiveToUtc :
                                          plmedia.Media.ActiveFromUtc.HasValue && !plmedia.Media.ActiveToUtc.HasValue ? plmedia.Media.ActiveFromUtc <= DateTime.UtcNow :
                                         plmedia.Media.ActiveFromUtc.HasValue || !plmedia.Media.ActiveToUtc.HasValue || DateTime.UtcNow <= plmedia.Media.ActiveToUtc )
                          select new MediaInfoModel
                          {
                              Id = plmedia.Media.Id,
                              Title = plmedia.Media.Name,
                              Url = plmedia.Media.Url,
                              Logo = plmedia.Media.FeaturedImage,
                              Thumbnail = plmedia.Media.FeaturedImage,
                              IsSharingAllowed = plmedia.Media.IsSharingAllowed,
                              SeoUrl = plmedia.Media.SeoUrl,
                              UniqueId = plmedia.Media.UniqueId,
                          }).ToList();
            return result;
        }

        #endregion
    }
}
