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
    /// <inheritdoc />
    public class TopicWriteService : ITopicWriteService
    {
        readonly TestStoriesContext _context;
        readonly IS3BucketService _s3BucketService;
        readonly ICloudTopicToolSeriesProvider _topicToolSeriesCloudSearch;
        readonly ICloudMediaSearchProvider _mediaCloudSearch;

        public TopicWriteService (TestStoriesContext context , IS3BucketService s3BucketService, ICloudTopicToolSeriesProvider topicToolSeriesCloudSearch, ICloudMediaSearchProvider mediaCloudSearch)
        {
            _context = context;
            _s3BucketService = s3BucketService;
            _topicToolSeriesCloudSearch = topicToolSeriesCloudSearch;
            _mediaCloudSearch = mediaCloudSearch;

        }
        
        /// <inheritdoc />
        public TopicWriteService(TestStoriesContext context, IS3BucketService s3BucketService, ICloudTopicToolSeriesProvider topicToolSeriesCloudSearch)
        {
            _context = context;
            _s3BucketService = s3BucketService;
            _topicToolSeriesCloudSearch = topicToolSeriesCloudSearch;
        }

        public async Task DeleteTopicByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Topic name cannot be empty");
            }

            var topic = _context.Topic.FirstOrDefault(x => x.Name.Trim() == name.Trim());
            if (topic == null)
            {
                throw new ArgumentException("Topic not found");
            }

            await RemoveTopicAsync(topic.Id);
        }

        public async Task<TopicModel> AddTopicAsync (AddTopicModel model)
        {
            Topic topic;
            if ( model.TopicName != null )
            {
                var isTopicExist = _context.Topic.Any(x => x.Name == model.TopicName);
                if ( isTopicExist )
                    throw new BusinessException("Topic Name already exist");
            }

            if ( model.ParentId != 0 && model.ParentId != null )
            {
                var isParentTopicExist = _context.Topic.Any(x => x.Id == model.ParentId);
                if ( !isParentTopicExist )
                    throw new BusinessException("Parent Topic not exist");
            }


            topic = await AddTopicAsync(new Topic
            {
                Name = model.TopicName ,
                ParentId = model.ParentId ,
                Description = !string.IsNullOrEmpty(model.Description)
                    ? model.Description
                    : string.Empty ,
                LogoMetadata = model.Logo != null ? model.Logo.FileName : string.Empty ,
                SeoUrl = Helper.SeoFriendlyUrl(model.TopicName)
            });

            var logoUrl = model.Logo != null ? await _s3BucketService.UploadFileByTypeToStorageAsync(model.Logo , topic.Id , EntityType.Topics , FileTypeEnum.Logo.ToString()) : string.Empty;
            await UpdateLogoAsync(topic , logoUrl);


            if ( topic == null )
            {
                throw new BusinessException("Can not add a new topic. Please, try again.");
            }

            var topicModel = new TopicToolSeriesModel
            {
                Id = topic.Id ,
                Title = topic.Name ,
                Description = topic.Description ,
                ParentTopic = await GetTopicName(topic.ParentId) ,
                Logo = topic.Logo ,
                SeoUrl = topic.SeoUrl ,
                FeaturedImage = topic.Logo,
                BannerImage = "" ,
                Thumbnail = "" ,
                AssignedTo = new List<string>() ,
                DateCreated = DateTime.UtcNow.ToUniversalTime() ,
                Link = "" ,
                Topics = new List<string>() ,
                ShowOnMenu = 0 ,
                ShowOnHomePage = 0 ,
                ItemType = "Topic" ,
                Type = "" ,
                Partner = ""
            };

            // Add to cloud
            var status = _topicToolSeriesCloudSearch.AddToCloud(topicModel);
            return new TopicModel
            {
                Id = topicModel.Id ,
                TopicName = topicModel.Title ,
                Description = topicModel.Description ,
                ParentId = topic.ParentId ,
                ParentTopic = topicModel.ParentTopic ,
                Logo = topicModel.Logo ,
                SeoUrl = topicModel.SeoUrl
            };
        }

        public async Task<TopicModel> EditTopicAsync (EditTopicModel model)
        {
            var dbTopic = await _context.Topic.SingleOrDefaultAsync(t => t.Id == model.Id);
            if ( dbTopic == null )
                throw new BusinessException("Topic not found");

            var logoUrl = dbTopic.Logo;
            var logoFileName = dbTopic.LogoMetadata;

            if ( model.TopicName != null )
            {
                var topicDetail = await _context.Topic.SingleOrDefaultAsync(x => x.Name == model.TopicName);
                if ( topicDetail != null )
                    if ( topicDetail.Id != model.Id )
                        throw new BusinessException("Name already exist try different name");
            }

            if ( model.ParentId != 0 && model.ParentId != null )
            {
                var isParentTopicExist = _context.Topic.Any(x => x.Id == model.ParentId);
                if ( !isParentTopicExist )
                    throw new BusinessException("Parent Topic not exist");
            }
            //Logo
            if ( model.Logo == null )
            {
                if ( !string.IsNullOrEmpty(dbTopic.Logo) && string.IsNullOrEmpty(model.LogoFileName) )
                {
                    await _s3BucketService.RemoveImageAsync(dbTopic.Logo);
                    logoUrl = string.Empty;
                    logoFileName = string.Empty;
                }
            }
            else
            {
                if ( !string.IsNullOrEmpty(dbTopic.Logo) )
                {
                    await _s3BucketService.RemoveImageAsync(dbTopic.Logo);
                }

                logoUrl = await _s3BucketService.UploadFileByTypeToStorageAsync(model.Logo , model.Id , EntityType.Topics , FileTypeEnum.Logo.ToString());
                logoFileName = model.Logo.FileName;
            }

            Topic topic;
            topic = await EditTopicAsync(new Topic
            {
                Id = model.Id ,
                Name = model.TopicName ,
                ParentId = model.ParentId ,
                Description = !string.IsNullOrEmpty(model.Description)
                    ? model.Description
                    : string.Empty ,
                Logo = logoUrl ,
                LogoMetadata = logoFileName
            });


            if ( topic == null )
            {
                throw new BusinessException("Can not edit the topic. Please, try again.");
            }

            var topicModel = new TopicToolSeriesModel
            {
                Id = topic.Id ,
                Title = topic.Name ,
                Description = topic.Description ,
                ParentTopic = await GetTopicName(topic.ParentId) ,
                Logo = topic.Logo ,
                SeoUrl = topic.SeoUrl ,
                FeaturedImage = topic.Logo,
                BannerImage = "" ,
                Thumbnail = "" ,
                AssignedTo = new List<string>() ,
                DateCreated = DateTime.UtcNow.ToUniversalTime() ,
                Topics = new List<string>() ,
                Link = "" ,
                ShowOnMenu = 0 ,
                ShowOnHomePage = 0 ,
                ItemType = "Topic" ,
                Type = "" ,
                Partner = ""
            };

            var status = _topicToolSeriesCloudSearch.UpdateToCloud(topicModel); //update on cloud
           
            return new TopicModel
            {
                Id = topicModel.Id ,
                TopicName = topicModel.Title ,
                Description = topicModel.Description ,
                ParentId = topic.ParentId ,
                ParentTopic = topicModel.ParentTopic ,
                Logo = topicModel.Logo ,
                SeoUrl = topicModel.SeoUrl
            };
        }

        public async Task RemoveTopicAsync (int id)
        {
            var dbTopic = await _context.Topic.SingleOrDefaultAsync(t => t.Id == id);
            if ( dbTopic == null )
                throw new BusinessException("");

            var fileRemoveResult = true;

            if ( !string.IsNullOrEmpty(dbTopic.Logo) )
            {
                fileRemoveResult = await _s3BucketService.RemoveImageAsync(dbTopic.Logo);
            }

            if ( !fileRemoveResult )
            {
                throw new BusinessException("Can not remove the logo file for the topic. Please, try again.");
            }

            var topicIds = _context.Topic.Where(topic => topic.ParentId == id).Select(topic => topic.Id).ToList();
            var mediaIds = _context.MediaTopic.Where(x => x.TopicId == id).Select(y => y.MediaId).ToList();
            var topicsToUpdate = GetTopics(topicIds);
            var topicMedias = GetTopicMedias(mediaIds , id);
            await RemoveTopic(id);

            if ( topicsToUpdate.Count > 0 )
            {
                _topicToolSeriesCloudSearch.BulkUpdateToCloud(topicsToUpdate);
            }
            if ( topicMedias.Count > 0 )
            {
                _mediaCloudSearch.BulkUpdateToCloud(topicMedias);
            }

        }

        public async Task UpdateCloudTopics ()
        {
            var topics = ( from topic in _context.Topic
                           join parentTopic in _context.Topic on topic.ParentId equals parentTopic.Id into topicgrp
                           from item in topicgrp.DefaultIfEmpty()
                           select new TopicToolSeriesModel
                           {
                               Id = topic.Id ,
                               Title = topic.Name ,
                               Description = topic.Description ,
                               ParentTopic = item.Name ?? "" ,
                               Logo = topic.Logo ,
                               SeoUrl = topic.SeoUrl ,
                               FeaturedImage = topic.Logo,
                               BannerImage = "" ,
                               Thumbnail = "" ,
                               AssignedTo = new List<string>() ,
                               DateCreated = DateTime.UtcNow.ToUniversalTime() ,
                               Topics = new List<string>() ,
                               Link = "" ,
                               ShowOnMenu = 0 ,
                               ShowOnHomePage = 0 ,
                               ItemType = "Topic" ,
                               Type = "" ,
                               Partner = ""
                           } ).ToList<dynamic>();

            _topicToolSeriesCloudSearch.BulkAddToCloud(topics);
        }

        public async Task MigrateDbTopicsToCloud ()
        {
            var status = string.Empty;
            var topics = ( from topic in _context.Topic
                           join parentTopic in _context.Topic on topic.ParentId equals parentTopic.Id into topicgrp
                           from item in topicgrp.DefaultIfEmpty()
                           select new TopicToolSeriesModel
                           {
                               Id = topic.Id ,
                               Title = topic.Name ,
                               Description = topic.Description ,
                               ParentTopic = item.Name ?? "" ,
                               Logo = topic.Logo ,
                               SeoUrl = topic.SeoUrl ,
                               FeaturedImage = topic.Logo,
                               BannerImage = "" ,
                               Thumbnail = "" ,
                               AssignedTo = new List<string>() ,
                               DateCreated = DateTime.UtcNow.ToUniversalTime() ,
                               Topics = new List<string>() ,
                               Link = "" ,
                               ShowOnMenu = 0 ,
                               ShowOnHomePage = 0 ,
                               ItemType = "Topic" ,
                               Type = "" ,
                               Partner = ""
                           } ).ToList<dynamic>();

            _topicToolSeriesCloudSearch.BulkAddToCloud(topics);
        }

        #region Private Methods

        private async Task<string> GetTopicName(int? topicId)
        {
            var name = string.Empty;
            if (!topicId.HasValue)
                return name;
            var topic = await _context.Topic.SingleOrDefaultAsync(t => t.Id == topicId.Value);
            if (topic != null)
                name = topic.Name;
            return name;
        }

        private async Task<Topic> AddTopicAsync (Topic entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        private async Task RemoveTopic (int topicId)
        {
            var dbTopic = await _context.Topic.SingleOrDefaultAsync(t => t.Id == topicId);
            if ( dbTopic != null )
            {
                using var transaction = _context.Database.BeginTransaction();
                try
                {
                    var subscriptionTopics = await _context.SubscriptionTopic.Where(s => s.TopicId == topicId).ToListAsync();
                    if (subscriptionTopics != null && subscriptionTopics.Any())
                    {
                        _context.SubscriptionTopic.RemoveRange(subscriptionTopics);
                    }

                    var mediaTopicList = await _context.MediaTopic.Where(m => m.TopicId == topicId).ToListAsync();
                    if (mediaTopicList != null && mediaTopicList.Any())
                    {
                        _context.MediaTopic.RemoveRange(mediaTopicList);
                    }

                    var topicToolList = await _context.ToolTopic.Where(m => m.TopicId == topicId).ToListAsync();
                    if (topicToolList.Count > 0)
                    {
                        _context.ToolTopic.RemoveRange(topicToolList);
                    }

                    var topics = await _context.Topic.Where(t => t.ParentId.HasValue && t.ParentId.Value == topicId)
                        .ToListAsync();
                    if (topics != null && topics.Any())
                    {
                        foreach (var topic in topics)
                        {
                            topic.ParentId = null;
                            _context.Topic.Update(topic);
                        }
                    }

                    _context.Topic.Remove(dbTopic);
                    await _context.SaveChangesAsync();

                    transaction.Commit();


                    var status = _topicToolSeriesCloudSearch.DeleteFromCloud("Topic" + topicId);
                    if (status.ToLower().Trim() != CloudStatus.success.ToString())
                    {
                        throw new Exception("Cannot delete topic from CloudSearch");
                    }

                    if (!string.IsNullOrEmpty(dbTopic.Logo))
                    {
                        await _s3BucketService.RemoveImageAsync(dbTopic.Logo);
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private async Task<Topic> EditTopicAsync (Topic entity)
        {
            var dbTopic = await _context.Topic.SingleOrDefaultAsync(t => t.Id == entity.Id);
            if ( dbTopic != null )
            {
                dbTopic.Name = entity.Name;
                dbTopic.Description = entity.Description;
                dbTopic.Logo = entity.Logo;
                dbTopic.LogoMetadata = entity.LogoMetadata;
                dbTopic.ParentId = entity.ParentId;
                _context.Topic.Update(dbTopic);
                await _context.SaveChangesAsync();
                return dbTopic;
            }
            return null;
        }

        private async Task UpdateLogoAsync (Topic entity , string logoUrl)
        {
            entity.Logo = logoUrl;
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

        private List<dynamic> GetTopicMedias (List<long> mediaIds , int topicId)
        {
            var topicMedias = ( from media in _context.Media
                                let topicNames = ( from x in _context.Media.Where(x => x.Id == media.Id)
                                                   join y in _context.MediaTopic
                                                   on x.Id equals y.MediaId
                                                   join t in _context.Topic
                                                   on y.TopicId equals t.Id
                                                   where y.TopicId != topicId
                                                   select t.Name ).ToList()
                                let tags = ( from mtags in _context.MediaTag.Where(x => x.MediaId == media.Id)
                                             join tagsItem in _context.Tag
                                             on mtags.TagId equals tagsItem.Id
                                             select tagsItem.Name ).ToList()
                                join mt in _context.MediaType on media.MediatypeId equals mt.Id
                                join ms in _context.MediaStatus on media.MediastatusId equals ms.Id
                                join upUser in _context.User on media.UploadUserId equals upUser.Id
                                join sr in _context.Series on media.SeriesId equals sr.Id into srGroup
                                from series in srGroup.DefaultIfEmpty()
                                join prt in _context.Partner on media.SourceId equals prt.Id into prtGroup
                                from partner in prtGroup.DefaultIfEmpty()
                                join pUser in _context.User on media.PublishUserId equals pUser.Id into pUserGroup
                                from PubUser in pUserGroup.DefaultIfEmpty()
                                where mediaIds.Contains(media.Id)
                                select new MediaCloudSearchEntity
                                {
                                    Id = media.Id ,
                                    Title = media.Name ,
                                    Description = media.Description ,
                                    LongDescription = media.LongDescription ,
                                    SeriesTitle = series.Name ,
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
                                    LastUpdatedDate = media.DateLastupdatedUtc ,
                                    IsSharingAllowed = Convert.ToInt32(media.IsSharingAllowed) ,
                                    thumbnail = media.Thumbnail ,
                                    seourl = media.SeoUrl,
                                    UniqueId = media.UniqueId ,
                                    IsDeleted = Convert.ToInt32(media.IsDeleted) ,
                                    IsVisibleOnGoogle = Convert.ToInt32(media.IsVisibleOnGoogle)
                                } ).ToList<dynamic>();
            return topicMedias;
        }

        private List<dynamic> GetTopics (List<int> parentTopicIds)
        {
            var topics = ( from topic in _context.Topic.Where(topic => parentTopicIds.Contains(topic.Id))
                           select new TopicToolSeriesModel
                           {
                               Id = topic.Id ,
                               Title = topic.Name ,
                               Description = topic.Description ,
                               ParentTopic = "" ,
                               Logo = topic.Logo ,
                               SeoUrl = topic.SeoUrl ,
                               FeaturedImage = topic.Logo,
                               BannerImage = "" ,
                               Thumbnail = "" ,
                               AssignedTo = new List<string>() ,
                               DateCreated = DateTime.UtcNow.ToUniversalTime() ,
                               Topics = new List<string>() ,
                               Link = "" ,
                               ShowOnMenu = 0 ,
                               ShowOnHomePage = 0 ,
                               ItemType = "Topic",
                               Type = "",
                               Partner = ""
                           } ).ToList<dynamic>();
            return topics;
        }

        #endregion
    }
}
