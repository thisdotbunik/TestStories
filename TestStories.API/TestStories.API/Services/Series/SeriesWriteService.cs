using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestStories.API.Services;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.CloudSearch.Service.Interface;
using TestStories.CloudSearch.Service.MediaEntity;
using TestStories.Common;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;

namespace TestStories.API.Concrete
{
    public class SeriesWriteService : ISeriesWriteService
    {
        private readonly TestStoriesContext _context;
        readonly IS3BucketService _s3BucketService;
        readonly ICloudTopicToolSeriesProvider _topicToolSeriesCloudSearch;
        readonly ICloudMediaSearchProvider _mediaCloudSearch;

        /// <inheritdoc />
        public SeriesWriteService(TestStoriesContext ctx, ICloudTopicToolSeriesProvider topicToolSeriesCloudSearch, ICloudMediaSearchProvider mediaCloudSearch, IS3BucketService s3BucketService)
        {
            _context = ctx;
            _topicToolSeriesCloudSearch = topicToolSeriesCloudSearch;
            _mediaCloudSearch = mediaCloudSearch;
            _s3BucketService = s3BucketService;
        }

        private IQueryable<SeriesType> SeriesTypes => _context.SeriesType;

        public async Task DeleteSeriesByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Series name cannot be empty");
            }

            var series = _context.Series.FirstOrDefault(x => x.Name.Trim() == name.Trim());
            if (series == null)
            {
                throw new ArgumentException("Series not found");
            }

            await RemoveSeriesAsync(series.Id);
        }

        public async Task<SeriesModel> AddSeriesAsync (AddSeriesModel model)
        {
            Series series;
            var isAddedOnCloud = string.Empty;
            var type = string.Empty;


            if ( model.SeriesTitle != null )
            {
                var isSeriesExist = _context.Series.Any(x => x.Name == model.SeriesTitle);
                if ( isSeriesExist )
                    throw new BusinessException("Series Name already exist");
            }

            if ( model.SeriesTypeId != 0 )
            {
                var seriesType = await SeriesTypes.FirstOrDefaultAsync(x => x.Id == model.SeriesTypeId);
                if ( seriesType is null )
                {
                    throw new BusinessException("Invalid Series Type");
                }
                type = seriesType.Name;
            }

            series = await AddSeriesInDb(new Series
            {
                Name = model.SeriesTitle ,
                SeriestypeId = model.SeriesTypeId ,
                Description = !string.IsNullOrEmpty(model.SeriesDescription) ? model.SeriesDescription : string.Empty ,
                LogoMetadata = model.SeriesLogo != null ? model.SeriesLogo.FileName : string.Empty ,
                FeaturedImageMetadata = model.SeriesImage != null ? model.SeriesImage.FileName : string.Empty ,
                HomepageBannerMetadata = model.HomepageBanner != null ? model.HomepageBanner.FileName : string.Empty ,
                SeoUrl = Helper.SeoFriendlyUrl(model.SeriesTitle),
                ShowOnMenu = model.ShowOnMenu,
                LogoSize = model.SeriesLogoSize,
                DescriptionColor = model.SeriesDescriptionColor,
            } , model.SuggestedMediaIds);

            var logoUrl = model.SeriesLogo != null ? await _s3BucketService.UploadFileByTypeToStorageAsync(model.SeriesLogo , series.Id , EntityType.Series , FileTypeEnum.Logo.ToString()) : string.Empty;
            var featuredImageUrl = model.SeriesImage != null ? await _s3BucketService.UploadFileByTypeToStorageAsync(model.SeriesImage , series.Id , EntityType.Series , FileTypeEnum.FeaturedImage.ToString()) : string.Empty;
            var homepageBannerUrl = model.HomepageBanner != null ? await _s3BucketService.UploadFileByTypeToStorageAsync(model.HomepageBanner , series.Id , EntityType.Series , FileTypeEnum.HomepageBanner.ToString()) : string.Empty;

            series.Logo = logoUrl;
            series.FeaturedImage = featuredImageUrl;
            series.HomepageBanner = homepageBannerUrl;
            await UpdateAsync(series);

            var seriesModel = new TopicToolSeriesModel
            {
                Id = series.Id ,
                Title = series.Name ,
                Description = series.Description ,
                ParentTopic = "" ,
                Logo = series.Logo ,
                SeoUrl = series.SeoUrl ,
                FeaturedImage = series.FeaturedImage ,
                BannerImage = series.HomepageBanner,
                Thumbnail = "" ,
                AssignedTo = new List<string>() ,
                DateCreated = DateTime.UtcNow.ToUniversalTime() ,
                Topics = new List<string>() ,
                Link = "" ,
                ShowOnMenu = Convert.ToInt32(series.ShowOnMenu),
                ShowOnHomePage = 0 ,
                ItemType = "Series" ,
                Type = type ,
                Partner = ""
            };

            // Add to cloud
            var status = _topicToolSeriesCloudSearch.AddToCloud(seriesModel);

            //if ( status.ToLower().Trim() != CloudStatus.success.ToString() )
            //{
            //    _logger.LogError($"Add:Received: request for Controller:SeriesController and Action: AddSeries cloud status {status}");
            //}

            return new SeriesModel
            {
                Id = series.Id,
                SeriesTitle = series.Name,
                SeriesDescription = series.Description,
                SeriesLogo = series.Logo ,
                SeriesImage = series.FeaturedImage,
                SeoUrl = series.SeoUrl,
                ShowOnMenu = series.ShowOnMenu,
                SeriesLogoSize = series.LogoSize,
                SeriesDescriptionColor = series.DescriptionColor
            };
        }
      
        public async Task UpdateAsync(Series entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<SeriesModel> EditSeriesAsync (EditSeriesModel model)
        {
            var dbSeries = await _context.Series.SingleOrDefaultAsync(t => t.Id == model.Id);
            if (dbSeries == null)
            {
                throw new BusinessException("Series not found");
            }

            var type = string.Empty;
            var logoUrl = dbSeries.Logo;
            var logoFileName = dbSeries.LogoMetadata;

            var featuredImageUrl = dbSeries.FeaturedImage;
            var featuredImageFileName = dbSeries.FeaturedImageMetadata;

            var homepageBannerUrl = dbSeries.HomepageBanner;
            var homepageBannerFileName = dbSeries.HomepageBannerMetadata;


            if ( model.SeriesTitle != null )
            {
                var seriesDetail = await _context.Series.SingleOrDefaultAsync(x => x.Name == model.SeriesTitle.Trim());
                if ( seriesDetail != null )
                    if ( seriesDetail.Id != model.Id )
                        throw new BusinessException("Name already exist try different name");
            }

            if ( model.SeriesTypeId != 0 )
            {
                var seriesType = await _context.SeriesType.FirstOrDefaultAsync(x => x.Id == model.SeriesTypeId);
                if ( seriesType is null )
                {
                    throw new BusinessException("Invalid Series Type");
                }
                type = seriesType.Name;
            }

            //Logo
            if ( model.SeriesLogo == null )
            {
                if ( !string.IsNullOrEmpty(dbSeries.Logo) && string.IsNullOrEmpty(model.LogoFileName) )
                {
                    await _s3BucketService.RemoveImageAsync(dbSeries.Logo);
                    logoUrl = string.Empty;
                    logoFileName = string.Empty;
                }
            }
            else
            {
                if ( !string.IsNullOrEmpty(dbSeries.Logo) )
                {
                    await _s3BucketService.RemoveImageAsync(dbSeries.Logo);
                }

                logoUrl = await _s3BucketService.UploadFileByTypeToStorageAsync(model.SeriesLogo , model.Id , EntityType.Series , FileTypeEnum.Logo.ToString());
                logoFileName = model.SeriesLogo.FileName;
            }

            //FeaturedImage
            if ( model.SeriesImage == null )
            {
                if ( !string.IsNullOrEmpty(dbSeries.FeaturedImage) && string.IsNullOrEmpty(model.ImageFileName) )
                {
                    await _s3BucketService.RemoveImageAsync(dbSeries.FeaturedImage);
                    featuredImageUrl = string.Empty;
                    featuredImageFileName = string.Empty;
                }
            }
            else
            {
                if ( !string.IsNullOrEmpty(dbSeries.FeaturedImage) )
                {
                    await _s3BucketService.RemoveImageAsync(dbSeries.FeaturedImage);
                }

                featuredImageUrl = await _s3BucketService.UploadFileByTypeToStorageAsync(model.SeriesImage , model.Id , EntityType.Series , FileTypeEnum.FeaturedImage.ToString());
                featuredImageFileName = model.SeriesImage.FileName;
            }

            //HomepageBanner
            if ( model.HomepageBanner == null )
            {
                if ( !string.IsNullOrEmpty(dbSeries.HomepageBanner) && string.IsNullOrEmpty(model.HomepageBannerName) )
                {
                    await _s3BucketService.RemoveImageAsync(dbSeries.HomepageBanner);
                }

                homepageBannerUrl = string.Empty;
                homepageBannerFileName = string.Empty;
            }
            else
            {
                if ( !string.IsNullOrEmpty(dbSeries.HomepageBanner) )
                {
                    await _s3BucketService.RemoveImageAsync(dbSeries.HomepageBanner);
                }

                homepageBannerUrl = await _s3BucketService.UploadFileByTypeToStorageAsync(model.HomepageBanner , model.Id , EntityType.Series , FileTypeEnum.FeaturedImage.ToString());
                homepageBannerFileName = model.HomepageBanner.FileName;
            }

            var series = await EditSeriesInDb(new Series
            {
                Id = model.Id ,
                SeriestypeId = model.SeriesTypeId ,
                Name = model.SeriesTitle ,
                Description = !string.IsNullOrEmpty(model.SeriesDescription) ? model.SeriesDescription : string.Empty ,
                Logo = logoUrl ,
                LogoMetadata = logoFileName ,
                FeaturedImage = featuredImageUrl ,
                FeaturedImageMetadata = featuredImageFileName,
                HomepageBanner = homepageBannerUrl ,
                HomepageBannerMetadata = homepageBannerFileName,
                ShowOnMenu = model.ShowOnMenu,
                DescriptionColor = model.SeriesDescriptionColor,
                LogoSize = model.SeriesLogoSize
            } , model.SuggestedMediaIds);


            var seriesModel = new TopicToolSeriesModel
            {
                Id = series.Id ,
                Title = series.Name ,
                Description = series.Description ,
                ParentTopic = "" ,
                Logo = series.Logo ,
                SeoUrl = series.SeoUrl ,
                FeaturedImage = series.FeaturedImage ,
                BannerImage = series.HomepageBanner ,
                Thumbnail = "" ,
                AssignedTo = new List<string>() ,
                DateCreated = DateTime.UtcNow.ToUniversalTime() ,
                Topics = new List<string>() ,
                Link = "" ,
                ShowOnMenu = Convert.ToInt32(series.ShowOnMenu),
                ShowOnHomePage = 0 ,
                ItemType = "Series" ,
                Type = type ,
                Partner = ""
            };

            // Add to cloud
            var status = _topicToolSeriesCloudSearch.UpdateToCloud(seriesModel);

            seriesModel.StatusAddedOnCloud = status;
            //if ( status.ToLower().Trim() != CloudStatus.success.ToString() )
            //{
            //    _logger.LogError($"Update:Received: request for controller: SeriesController and action: EditSeriesAsync cloud status is: {status} and userId- is: {GetUserId()}");
            //}

            return new SeriesModel
            {
                Id = series.Id,
                SeriesTitle = series.Name,
                SeriesDescription = series.Description,
                SeriesLogo = series.Logo,
                SeriesImage = series.FeaturedImage,
                SeoUrl = series.SeoUrl,
                ShowOnMenu = series.ShowOnMenu,
                SeriesDescriptionColor = series.DescriptionColor,
                SeriesLogoSize = series.LogoSize
            };
        }
       
        public async Task RemoveSeriesAsync (int seriesId)
        {
            var series = await _context.Series.SingleOrDefaultAsync(t => t.Id == seriesId);
            if (series == null)
                throw new BusinessException("Series not found");

            await RemoveSeriesFromDb(seriesId);
            var mediaIds = _context.Media.Where(media => media.SeriesId == seriesId).Select(media => media.Id).ToList();
            if ( mediaIds.Count > 0 )
            {
                UpdateSeriesMediasAtCloud(mediaIds);
            }

        }

        public async Task UpdateCloudSeries()
        {
            var series = (from serie in _context.Series.Include(x => x.Seriestype)
                          select new TopicToolSeriesModel
                          {
                              Id = serie.Id,
                              Title = serie.Name,
                              Description = serie.Description,
                              ParentTopic = "",
                              Logo = serie.Logo,
                              SeoUrl = serie.SeoUrl,
                              FeaturedImage = serie.FeaturedImage,
                              BannerImage = serie.HomepageBanner,
                              Thumbnail = "",
                              AssignedTo = new List<string>(),
                              DateCreated = DateTime.UtcNow.ToUniversalTime(),
                              Topics = new List<string>(),
                              Link = "",
                              ShowOnMenu = Convert.ToInt32(serie.ShowOnMenu),
                              ShowOnHomePage = 0,
                              ItemType = "Series",
                              Type = serie.Seriestype.Name,
                              Partner = ""
                          }).ToList<dynamic>();

            _topicToolSeriesCloudSearch.BulkUpdateToCloud(series);
        }

        public async Task MigrateDbSeriesToCloud()
        {
            var series = (from serie in _context.Series.Include(x => x.Seriestype)
                          select new TopicToolSeriesModel
                          {
                              Id = serie.Id,
                              Title = serie.Name,
                              Description = serie.Description,
                              ParentTopic = "",
                              Logo = serie.Logo,
                              SeoUrl = serie.SeoUrl,
                              FeaturedImage = serie.FeaturedImage,
                              BannerImage = serie.HomepageBanner,
                              Thumbnail = "",
                              AssignedTo = new List<string>(),
                              DateCreated = DateTime.UtcNow.ToUniversalTime(),
                              Topics = new List<string>(),
                              Link = "",
                              ShowOnMenu = 0,
                              ShowOnHomePage = 0,
                              ItemType = "Series",
                              Type = serie.Seriestype.Name,
                              Partner = ""
                          }).ToList<dynamic>();

            _topicToolSeriesCloudSearch.BulkAddToCloud(series);
        }

        #region Private Methods

        private async Task<Series> AddSeriesInDb(Series entity, string suggestedMediaIds)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                await _context.AddAsync(entity);

                if (!string.IsNullOrEmpty(suggestedMediaIds))
                {
                    var mediaIds = suggestedMediaIds.Split(',').Select(long.Parse).ToList();
                    var seriesMedias = new List<SeriesMedia>();
                    foreach (var mediaId in mediaIds)
                    {
                        var isMediaExist = _context.Media.Any(media => media.Id == mediaId);
                        if (isMediaExist)
                        {
                            seriesMedias.Add(new SeriesMedia { SeriesId = entity.Id, MediaId = mediaId });
                        }
                    }
                    _context.SeriesMedia.AddRange(seriesMedias);
                }
                await _context.SaveChangesAsync();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
            }
            return entity;
        }

        private async Task<Series> EditSeriesInDb(Series entity, string suggestedMediaIds)
        {
            var dbSeries = await _context.Series.Include(series => series.SeriesMedia).SingleOrDefaultAsync(series => series.Id == entity.Id);
            if (dbSeries == null) return null;
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                dbSeries.Name = entity.Name;
                dbSeries.SeriestypeId = entity.SeriestypeId;
                dbSeries.Description = entity.Description;
                dbSeries.Logo = entity.Logo;
                dbSeries.LogoMetadata = entity.LogoMetadata;
                dbSeries.FeaturedImage = entity.FeaturedImage;
                dbSeries.FeaturedImageMetadata = entity.FeaturedImageMetadata;
                dbSeries.HomepageBanner = entity.HomepageBanner;
                dbSeries.HomepageBannerMetadata = entity.HomepageBannerMetadata;
                dbSeries.ShowOnMenu = entity.ShowOnMenu;
                dbSeries.DescriptionColor = entity.DescriptionColor;
                dbSeries.LogoSize = entity.LogoSize;
                _context.Series.Update(dbSeries);

                if (!string.IsNullOrEmpty(suggestedMediaIds))
                {
                    var currentMediaIds = suggestedMediaIds.Split(',').Select(long.Parse).ToList();
                    var preMediaIds = dbSeries.SeriesMedia.Select(x => x.MediaId).ToList();
                    var addedMediaIds = currentMediaIds.Where(preMediaId => preMediaIds.All(currentMediaId => currentMediaId != preMediaId)).ToList();
                    var deletedMediaIds = preMediaIds.Where(currentMediaId => currentMediaIds.All(preMediaId => preMediaId != currentMediaId)).ToList();
                    var seriesMedias = new List<SeriesMedia>();
                    foreach (var mediaId in addedMediaIds)
                    {
                        var isMediaExist = _context.Media.Any(media => media.Id == mediaId);
                        if (isMediaExist)
                        {
                            seriesMedias.Add(new SeriesMedia { SeriesId = entity.Id, MediaId = mediaId });
                        }
                    }
                    _context.SeriesMedia.AddRange(seriesMedias);

                    var deleteMedias = _context.SeriesMedia.Where(seriesMedia => deletedMediaIds.Contains(seriesMedia.MediaId)).ToList();
                    _context.SeriesMedia.RemoveRange(deleteMedias);
                }
                else
                {
                    var seriesMedias = _context.SeriesMedia.Where(seriesMedia => seriesMedia.SeriesId == entity.Id).ToList();
                    _context.RemoveRange(seriesMedias);
                }
                await _context.SaveChangesAsync();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
            }
            return dbSeries;
        }

        private async Task RemoveSeriesFromDb(int seriesId)
        {
            var dbSeries = await _context.Series.SingleOrDefaultAsync(t => t.Id == seriesId);
            if (dbSeries != null)
            {
                using var transaction = _context.Database.BeginTransaction();
                try
                {
                    var subscriptionTopics = await _context.SubscriptionSeries.Where(s => s.SeriesId == seriesId).ToListAsync();
                    if (subscriptionTopics.Count > 0)
                    {
                        _context.SubscriptionSeries.RemoveRange(subscriptionTopics);
                    }

                    var seriesMedias = await _context.SeriesMedia.Where(s => s.SeriesId == seriesId).ToListAsync();
                    if (seriesMedias.Count > 0)
                    {
                        _context.SeriesMedia.RemoveRange(seriesMedias);
                    }

                    var mediaList = await _context.Media.Where(m => m.SeriesId.HasValue && m.SeriesId == seriesId).ToListAsync();
                    if (mediaList.Count > 0)
                    {
                        foreach (var media in mediaList)
                        {
                            media.SeriesId = null;
                            _context.Media.Update(media);
                        }
                    }
                    var series = await _context.Series.Where(t => t.Id == seriesId).ToListAsync();
                    if (series.Count > 0)
                    {
                        _context.Series.RemoveRange(series);

                    }


                    await _context.SaveChangesAsync();
                    transaction.Commit();

                    var status = _topicToolSeriesCloudSearch.DeleteFromCloud("Series" + seriesId);
                    if (status.ToLower().Trim() != CloudStatus.success.ToString())
                    {
                        throw new Exception("Cannot delete topic from CloudSearch");
                    }


                    if (!string.IsNullOrEmpty(dbSeries.Logo))
                    {
                        await _s3BucketService.RemoveImageAsync(dbSeries.Logo);
                    }

                    if (!string.IsNullOrEmpty(dbSeries.FeaturedImage))
                    {
                        await _s3BucketService.RemoveImageAsync(dbSeries.FeaturedImage);
                    }
                }

                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
           
        }

        private void UpdateSeriesMediasAtCloud (List<long> mediaIds)
        {
            var seriesMedias = ( from media in _context.Media.Where(media => mediaIds.Contains(media.Id))
                                 let topicNames = ( from x in _context.Media.Where(x => x.Id == media.Id)
                                                    join y in _context.MediaTopic
                                                    on x.Id equals y.MediaId
                                                    join t in _context.Topic
                                                    on y.TopicId equals t.Id
                                                    select t.Name ).ToList()
                                 let tags = ( from mtags in _context.MediaTag.Where(x => x.MediaId == media.Id)
                                              join tagsItem in _context.Tag
                                              on mtags.TagId equals tagsItem.Id
                                              select tagsItem.Name ).ToList()
                                 join mt in _context.MediaType on media.MediatypeId equals mt.Id
                                 join ms in _context.MediaStatus on media.MediastatusId equals ms.Id
                                 join upUser in _context.User on media.UploadUserId equals upUser.Id
                                 join prt in _context.Partner on media.SourceId equals prt.Id into prtGroup
                                 from partner in prtGroup.DefaultIfEmpty()
                                 join pUser in _context.User on media.PublishUserId equals pUser.Id into pUserGroup
                                 from PubUser in pUserGroup.DefaultIfEmpty()
                                 select new MediaCloudSearchEntity
                                 {
                                     Id = media.Id ,
                                     Title = media.Name ,
                                     Description = media.Description ,
                                     LongDescription = media.LongDescription ,
                                     SeriesTitle = "" ,
                                     TopicTitle = topicNames ,
                                     Tags = tags ,
                                     Status = ms.Name ,
                                     MediaType = mt.Name ,
                                     Date = ms.Name.ToString().ToLower() == "published" ? DateTime.Now.ToString() : "" ,
                                     Source = partner.Name ,
                                     UploadedBy = upUser.Name ,
                                     PublishedBy = PubUser.Name ,
                                     Logo = media.FeaturedImage ,
                                     IsPrivate = Convert.ToInt32(media.IsPrivate) ,
                                     ActiveFrom = media.ActiveFromUtc ,
                                     ActiveTo = media.ActiveToUtc ,
                                     IsSharingAllowed = Convert.ToInt32(media.IsSharingAllowed) ,
                                     thumbnail = media.Thumbnail ,
                                     seourl = media.SeoUrl,
                                     LastUpdatedDate = DateTime.UtcNow,
                                     UniqueId = media.UniqueId,
                                     IsDeleted = Convert.ToInt32(media.IsDeleted),
                                     IsVisibleOnGoogle = Convert.ToInt32(media.IsVisibleOnGoogle)
                                 } ).ToList<dynamic>();
            _mediaCloudSearch.BulkUpdateToCloud(seriesMedias);
        }
        #endregion
    }
}
