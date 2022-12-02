using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestStories.API.Models.ResponseModels;
using TestStories.Common.Configurations;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;
using Newtonsoft.Json;

namespace TestStories.API.Services
{
    public class CommonReadService : ICommonReadService
    {
        private readonly TestStoriesContext _context;
        private readonly IS3BucketService _s3BucketService;
        private readonly ImageSettings _imageSettings;
        private readonly AppSettings _appSettings;
        private readonly ILogger<CommonReadService> _logger;

        public CommonReadService(TestStoriesContext context, IS3BucketService s3BucketService, 
            IOptions<ImageSettings> imageSettings, IOptions<AppSettings> appSettings, ILogger<CommonReadService> logger)
        {
            _context = context;
            _s3BucketService = s3BucketService;
            _imageSettings = imageSettings.Value;
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public async Task<Page<ContextChange>> GetContentChanges(DateTime? publishedDate, int offset, int limit, bool isFilteredByMediaId)
        {
            var topicsQuery = _context.Media.Where(x => x.MediaTopic.Count > 0 && x.MediastatusId == (int)MediaStatusEnum.Published && !x.IsDeleted && 
            ( x.ActiveFromUtc.HasValue ? x.ActiveFromUtc >= publishedDate && DateTime.UtcNow >= x.ActiveFromUtc :
             !x.ActiveFromUtc.HasValue && !x.ActiveToUtc.HasValue && x.DatePublishedUtc >= publishedDate && x.DatePublishedUtc <= DateTime.UtcNow ))
             .Include(x => x.MediaTopic).ThenInclude(x => x.Topic).ThenInclude(x => x.SubscriptionTopic).ThenInclude(x => x.User)
             .SelectMany(x => x.MediaTopic, (entity, mediaTopic) => new
             {
                 Media = entity,
                 mediaTopic.Topic.SubscriptionTopic
             })
             .SelectMany(x => x.SubscriptionTopic, (entity, subscription) => new
              {
                 entity.Media,
                 subscription.Topic,
                 subscription.User
              }).Select(x=> new
              {
                  MediaId = x.Media.Id,
                  x.Media.Name,
                  x.Media.Description,
                  ThumbnailUrl = x.Media.FeaturedImage ?? x.Media.Thumbnail,
                  Url =  $"https://{_appSettings.ClientUiDomain}/media/{x.Media.UniqueId}?video={x.Media.Id}",
                  Link = $"https://{_appSettings.ClientUiDomain}/topic/{x.Topic.SeoUrl}?id={x.Topic.Id}" ,
                  Type = "Topic",
                  UserId = x.User.Id,
                  UserStatusId = x.User.UserstatusId,
                  x.Media.IsPrivate,
                  SeriesId = 0,
                  TopicId = x.Topic.Id,
                  UserName = x.User.Name,
                  UserEmail = x.User.Email,
                  x.Media.DatePublishedUtc,
                  SeriesOrTopicName = x.Topic.Name,
                  Filter = x.Media.Id + "_" + x.User.Id
              }).Where(x=>x.UserStatusId == (int)UserStatusEnum.Active && x.IsPrivate == false).ToList();

            var seriesQuery = _context.Media.Where(x => x.SeriesId.HasValue && x.MediastatusId == (int)MediaStatusEnum.Published && 
            ( x.ActiveFromUtc.HasValue ? x.ActiveFromUtc >= publishedDate && DateTime.UtcNow >= x.ActiveFromUtc :
             !x.ActiveFromUtc.HasValue && !x.ActiveToUtc.HasValue && x.DatePublishedUtc >= publishedDate && x.DatePublishedUtc <= DateTime.UtcNow ))
             .Include(x => x.Series).ThenInclude(x => x.SubscriptionSeries).ThenInclude(x => x.User)         
             .SelectMany(x => x.Series.SubscriptionSeries, (entity, subscription) => new
             {
                 Media = entity,
                 subscription.Series,
                 subscription.User
             }).Select(x => new 
             { 
                 MediaId = x.Media.Id,
                 x.Media.Name,
                 x.Media.Description,
                 ThumbnailUrl = x.Media.FeaturedImage ?? x.Media.Thumbnail,
                 Url =  $"https://{_appSettings.ClientUiDomain}/media/{x.Media.UniqueId}?video={x.Media.Id}",
                 Link = $"https://{_appSettings.ClientUiDomain}/series/{x.Series.SeoUrl}?id={x.Series.Id}",
                 Type = "Series",
                 UserId = x.User.Id,
                 UserStatusId = x.User.UserstatusId,
                 x.Media.IsPrivate,
                 SeriesId = x.Series.Id,
                 TopicId = 0,
                 UserName = x.User.Name,
                 UserEmail = x.User.Email,
                 x.Media.DatePublishedUtc,
                 SeriesOrTopicName = x.Series.Name,
                 Filter = x.Media.Id + "_" + x.User.Id
             }).Where(x=>x.UserStatusId == (int)UserStatusEnum.Active && x.IsPrivate == false).ToList();

            var combinedMedias = topicsQuery.Union(seriesQuery);
            var groupMedia = combinedMedias.GroupBy(x => x.UserEmail);
            var medias = new List<ContextChange>();
            foreach (var media in groupMedia)
            {
                var groupByMedia = media.GroupBy(x => x.MediaId);
                foreach ( var gpMedia in groupByMedia )
                {
                    var topicSeries = new List<TopicSeries>();
                    foreach ( var item in gpMedia )
                    {
                        var topicSerie = new TopicSeries()
                        {
                            Type = item.Type ,
                            Name = item.SeriesOrTopicName ,
                            TopicLink = item.Link
                        };
                        if ( item.TopicId != 0 )
                        {
                            topicSerie.Id = item.TopicId;
                        }
                        else
                        {
                            topicSerie.Id = item.SeriesId;
                        }
                        topicSeries.Add(topicSerie);
                    }
                    var item1 = new ContextChange()
                    {
                        MediaId = gpMedia.FirstOrDefault().MediaId ,
                        Name = gpMedia.FirstOrDefault().Name ,
                        Description = gpMedia.FirstOrDefault().Description ,
                        Url = gpMedia.FirstOrDefault().Url ,
                        UserId = gpMedia.FirstOrDefault().UserId ,
                        TopicSeries = topicSeries ,
                        Logo = !string.IsNullOrEmpty(gpMedia.FirstOrDefault().ThumbnailUrl) ? _s3BucketService.RetrieveImageCDNUrl(gpMedia.FirstOrDefault().ThumbnailUrl) : string.Empty ,
                        UserName = gpMedia.FirstOrDefault().UserName ,
                        EmailAddress = gpMedia.FirstOrDefault().UserEmail ,
                        DatePublishedUtc = gpMedia.FirstOrDefault().DatePublishedUtc.Value ,
                    };
                    medias.Add(item1);
                }
            }
            medias = medias.Distinct().OrderBy(x => x.UserId).ThenByDescending(media => media.DatePublishedUtc).ToList();
            if ( isFilteredByMediaId )
            {
                medias = medias.GroupBy(x => x.MediaId).Select(g => g.First()).ToList();
            }
            var total = medias.Count;

            if (offset > 0)
            {
                medias = medias.Skip(offset).ToList();
            }

            if (limit > 0)
            {
                medias = medias.Take(limit).ToList();
            }

            var result = new Page<ContextChange>()
            {
               Offset = offset,
               Limit = limit,
               Total = total,
               Items = medias
            };

            return result;
        }
        public async Task<CommonApis> Lookup (LookupType lookupType)
        {
            var topicItems = new List<TopicLookup>();
            var seriesItems = new List<SeriesLookup>();
            var seriesTypes = new List<SeriesTypeLookup>();
            var tagItems = new List<TagLookup>();
            var sourceItems = new List<SourceLookup>();
            var userType = new List<UserTypes>();
            var userStatus = new List<Status>();
            var mediaType = new List<MediaTypes>();
            var mediaStatus = new List<Media_Status>();
            var editors = new List<Editors>();
            var publisher = new List<Publisher>();
            var distribtionPartners = new List<SourceLookup>();
            var activeContentPartners = new List<SourceLookup>();
            var experimentStatus = new List<Models.ResponseModels.ExperimentStatus>();
          
            if ( lookupType.Topic )
            {
                topicItems = ( from x in _context.Topic
                               select new TopicLookup
                               {
                                   Id = x.Id ,
                                   Name = x.Name
                               } ).OrderBy(x => x.Id).ToList();
            }
            if ( lookupType.Series )
            {

                seriesItems = ( from x in _context.Series
                                select new SeriesLookup
                                {
                                    Id = x.Id ,
                                    Name = x.Name
                                } ).OrderBy(x => x.Id).ToList();
            }

            if ( lookupType.SeriesType )
            {

                seriesTypes = ( from x in _context.SeriesType
                                select new SeriesTypeLookup
                                {
                                    Id = x.Id ,
                                    Name = x.Name
                                } ).OrderBy(x => x.Id).ToList();
            }

            if ( lookupType.Tags )
            {
                tagItems = ( from x in _context.Tag
                             select new TagLookup
                             {
                                 Id = x.Id ,
                                 Name = x.Name
                             } ).OrderBy(x => x.Id).ToList();
            }


            if ( lookupType.Source )
            {

                sourceItems = ( from x in _context.Partner
                                join y in _context.PartnerPartnerType.Where(p => p.PartnertypeId == 1)
                                on x.Id equals y.PartnerId
                                select new SourceLookup
                                {
                                    Id = x.Id ,
                                    Name = x.Name
                                } ).OrderBy(x => x.Id).ToList();
            }


            if ( lookupType.ActiveContentPartners )
            {

                activeContentPartners = ( from x in _context.Partner
                                          join y in _context.PartnerPartnerType.Where(p => p.PartnertypeId == 1)
                                          on x.Id equals y.PartnerId
                                          where x.IsArchived == false
                                          select new SourceLookup
                                          {
                                              Id = x.Id ,
                                              Name = x.Name
                                          } ).OrderBy(x => x.Id).ToList();
            }
            if ( lookupType.DistributionPartner )
            {

                distribtionPartners = ( from x in _context.Partner
                                        join y in _context.PartnerPartnerType.Where(p => p.PartnertypeId == 2)
                                        on x.Id equals y.PartnerId
                                        select new SourceLookup
                                        {
                                            Id = x.Id ,
                                            Name = x.Name
                                        } ).OrderBy(x => x.Id).ToList();
            }

            if ( lookupType.UserType )
            {
                userType = ( from x in _context.UserType
                             select new UserTypes
                             {
                                 Id = x.Id ,
                                 Name = x.Name
                             } ).OrderBy(x => x.Id).ToList();

            }

            if ( lookupType.UserStatus )
            {

                userStatus = ( from x in _context.UserStatus
                               select new Status
                               {
                                   Id = x.Id ,
                                   Name = x.Name
                               } ).OrderBy(x => x.Id).ToList();

            }
            _logger.LogDebug($"fulfilled request for commoncontroller userStatus: {userStatus}");
            if ( lookupType.MediaType )
            {

                mediaType = ( from x in _context.MediaType
                              select new MediaTypes
                              {
                                  Id = x.Id ,
                                  Name = x.Name
                              } ).OrderBy(x => x.Id).ToList();
            }
            _logger.LogDebug($"fulfilled request for commoncontroller mediaType: {mediaType}");

            if ( lookupType.MediaStatus )
            {

                mediaStatus = ( from x in _context.MediaStatus
                                select new Media_Status
                                {
                                    Id = x.Id ,
                                    Name = x.Name
                                } ).OrderBy(x => x.Id).ToList();
            }

            if ( lookupType.Editors )
            {
                editors = ( from x in _context.User.Where(x => x.UsertypeId != 4)
                            select new Editors
                            {
                                Id = x.Id ,
                                Name = x.Name
                            } ).OrderBy(x => x.Name).ToList();
            }
            _logger.LogDebug($"fulfilled request for commoncontroller: {editors}");
            if ( lookupType.Publishers )
            {
                publisher = ( from x in _context.User.Where(x => x.UsertypeId == 1)
                              select new Publisher
                              {
                                  Id = x.Id ,
                                  Name = x.Name
                              } ).OrderBy(x => x.Name).ToList();
            }


            _logger.LogDebug($"fulfilled request for commoncontroller: {publisher}");


            if ( lookupType.ExperimentStatus )
            {
                experimentStatus = ( from x in _context.ExperimentStatus
                                     select new Models.ResponseModels.ExperimentStatus
                                     {
                                         Id = x.Id ,
                                         Name = x.Name
                                     } ).OrderBy(x => x.Name).ToList();
            }

            return new CommonApis
            {
                Series = seriesItems ,
                SeriesType = seriesTypes ,
                Topics = topicItems ,
                Tags = tagItems ,
                Sources = sourceItems ,
                UserType = userType ,
                UserStatus = userStatus ,
                MediaTypes = mediaType ,
                MediaStatus = mediaStatus ,
                Editor = editors ,
                Publishers = publisher ,
                DistributionPartners = distribtionPartners ,
                ActiveContentPartners = activeContentPartners ,
                ExperimentStatus = experimentStatus
            };
        }
    }


    public class ContextChange
    {
        [JsonProperty(propertyName: "mediaId")]
        public long MediaId { get; set; }

        [JsonProperty(propertyName: "name")]
        public string Name { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }

        [JsonProperty(propertyName: "logo")]
        public string Logo { get; set; }

        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }

        [JsonProperty(propertyName: "userId")]
        public int UserId { get; set; }

        [JsonProperty("topicSeries")]
        public List<TopicSeries> TopicSeries { get; set; }

        [JsonProperty(propertyName: "userName")]
        public string UserName { get; set; }

        [JsonProperty(propertyName: "emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty(propertyName: "datePublishedUtc")]
        public DateTime DatePublishedUtc { get; set; }

    }

    public class TopicSeries
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("link")]
        public string TopicLink { get; set; }
    }


    public class Page<T>
    {
        [JsonProperty(propertyName: "total")]
        public int Total { get; set; }

        [JsonProperty(propertyName: "offset")]
        public int Offset { get; set; }

        [JsonProperty(propertyName: "limit")]
        public int Limit { get; set; }

        [JsonProperty(propertyName: "items")]
        public List<T> Items { get; set; } = new List<T>();
    }
}
