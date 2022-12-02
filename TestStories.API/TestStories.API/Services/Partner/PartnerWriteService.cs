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
    public class PartnerWriteService : IPartnerWriteService
    {
        private readonly TestStoriesContext _context;
        private readonly IS3BucketService _s3BucketService;
        private readonly ICloudMediaSearchProvider _mediaCloudSearchProvider;
        private readonly ICloudTopicToolSeriesProvider _topicToolSeriesCloudProvider;

        /// <inheritdoc />
        public PartnerWriteService (TestStoriesContext ctx , IS3BucketService s3BucketService , ICloudMediaSearchProvider mediaCloudSearchProvider , ICloudTopicToolSeriesProvider topicToolSeriesCloudProvider)
        {
            _context = ctx;
            _s3BucketService = s3BucketService;
            _mediaCloudSearchProvider = mediaCloudSearchProvider;
            _topicToolSeriesCloudProvider = topicToolSeriesCloudProvider;
        }
        /// <inheritdoc />

        public async Task RemovePartnerAsync (int id)
        {
            var dbSeries = await _context.Partner.SingleOrDefaultAsync(t => t.Id == id);
            if ( dbSeries == null )
                throw new BusinessException("Series not found");

            var mediaIds = await _context.Media.Where(x => x.SourceId == id).ToListAsync();
            var toolIds = await _context.Tool.Where(x => x.PartnerId == id).ToListAsync();

            // update user on DB
            await updateUserStatus(id);

            // remove partner from DB
            await RemovePartner(id);

            //update media status at cloud
            if (mediaIds.Count > 0 )
            {
                foreach ( var item in mediaIds )
                {
                    var media = _mediaCloudSearchProvider.GetCurrentItemFromCloud(item.Id.ToString() , 10 , 1).FirstOrDefault();
                    if ( media != null )
                    {
                        var clousearchdentity = new MediaCloudSearchEntity
                        {
                            Id = Convert.ToInt64(media.id) ,
                            Title = media.title ,
                            Description = media.description ,
                            LongDescription = media.longdescription,
                            SeriesTitle = media.seriestitle ,
                            TopicTitle = media.topictitle ,
                            Tags = media.tags ,
                            Status = "Archived" ,//media.status,
                            MediaType = media.mediatype ,
                            Date = media.date ,
                            Source = media.source ,
                            UploadedBy = media.uploadedby ,
                            PublishedBy = media.publisedby ,
                            Logo = media.logo ,//featured image
                            ActiveFrom = media.activefrom,
                            ActiveTo = media.activeto,
                            IsPrivate = media.isprivate,
                            thumbnail = media.thumbnail,
                            seourl = media.seourl,
                            LastUpdatedDate = media.lastupdateddate,
                            UniqueId = media.uniqueid,
                            IsSharingAllowed = media.issharingallowed,
                            IsDeleted = media.isdeleted,
                            IsVisibleOnGoogle = media.isvisibleongoogle,
                        };
                        var _statusonCloud = _mediaCloudSearchProvider.UpdateToCloud(clousearchdentity);
                    }
                }
            }

            if ( toolIds.Count > 0 )
            {
                foreach ( var item in toolIds )
                {
                    var tool = _topicToolSeriesCloudProvider.GetCloudToolById(item.Id.ToString()).FirstOrDefault();
                    if ( tool != null )
                    {
                        var toolModel = new TopicToolSeriesModel
                        {
                            Id = item.Id ,
                            Title = tool.title ?? "" ,
                            Description = tool.description ?? "" ,
                            ParentTopic = "" ,
                            Logo = "" ,
                            SeoUrl = "" ,
                            FeaturedImage = tool.featuredimage ?? "" ,
                            Thumbnail = "" ,
                            AssignedTo = tool.assignedto ?? new List<string>() ,
                            DateCreated = tool.datecreated ,
                            Topics = tool.topics ?? new List<string>() ,
                            Link = tool.link ?? "" ,
                            ShowOnMenu = tool.showonmenu ,
                            ShowOnHomePage = tool.showonhomepage ,
                            ItemType = tool.itemtype ?? "" ,
                            Type = tool.type ?? "" ,
                            Partner = ""
                        };
                        var _statusonCloud = _topicToolSeriesCloudProvider.UpdateToCloud(toolModel);
                    }
                }
            }
        }

        public async Task ExipreDistributionPartner (List<int> partnerId)
        {
            try
            {
                foreach ( var item in partnerId )
                {
                    var partnerMedia = await _context.PartnerMedia.SingleOrDefaultAsync(t => t.Id == item);
                    partnerMedia.IsExpired = true;
                    _context.PartnerMedia.Update(partnerMedia);
                }
                await _context.SaveChangesAsync();
            }
            catch ( Exception )
            {

            }
        }

        public async Task updateEndDateOfDistributionPartner (int distributionId , DateTime endDate)
        {
            var partnerMedia = await _context.PartnerMedia.SingleOrDefaultAsync(t => t.Id == distributionId);
            if ( partnerMedia == null )
                throw new BusinessException("Partner not found");

            partnerMedia.EndDateUtc = endDate;
            _context.PartnerMedia.Update(partnerMedia);
            await _context.SaveChangesAsync();
        }

        public async Task UnarchivePartner (int id)
        {
            var partner = await _context.Partner.SingleOrDefaultAsync(t => t.Id == id);
            if ( partner == null )
                throw new BusinessException("Partner not found");

            partner.IsArchived = false;
            _context.Partner.Update(partner);
            await _context.SaveChangesAsync();
        }

        public async Task<PartnerResponseModel> AddPartnerAsync (AddPartnerViewModel entity)
        {
            var partnerType = entity.PartnerTypeIds != null ? entity.PartnerTypeIds.Split(",").ToList() : null;
            var partnerTypeIds = new List<byte>();
            if ( partnerType != null )
            {
                foreach ( var item in partnerType )
                {
                    var partnerId = Convert.ToByte(item);
                    partnerTypeIds.Add(partnerId);
                }
            }
            var isPartnerExist = _context.Partner.Where(x => x.Name == entity.Name).Any();
            if ( isPartnerExist )
            {
                throw new BusinessException("Partner Already exist");
            }


            var partner = new Partner
            {
                Name = entity.Name ,
                Description = entity.Description ,
                ShowOnPartnerPage = entity.ShowOnPartnerPage ,
                IsArchived = entity.IsArchived ,
                DateAddedUtc = DateTime.UtcNow ,
                LogoMetadata = entity.Logo != null ? entity.Logo.FileName : string.Empty ,
                Link = entity.Link
            };

            partner = await AddPartner(partner , partnerTypeIds);

            var filePath = entity.Logo != null ? await _s3BucketService.UploadFileByTypeToStorageAsync(entity.Logo , partner.Id , EntityType.None , FileTypeEnum.Logo.ToString()) : string.Empty;
            await UpdateLogoAsync(partner , filePath);

            return new PartnerResponseModel
            {
                Id = partner.Id ,
                Name = partner.Name ,
                Description = partner.Description ,
                ShowOnPartnerPage = partner.ShowOnPartnerPage ,
                IsArchived = partner.IsArchived ,
            };
        }

        public async Task<PartnerResponseModel> EditPartnerAsync (int partnerId , EditPartnerViewModel model)
        {
            var dbPartner = await _context.Partner.SingleOrDefaultAsync(t => t.Id == partnerId);
            var logoUrl = dbPartner.Logo;
            var logoFileName = dbPartner.LogoMetadata;
            var filePath = string.Empty;
            var partnerType = model.PartnerTypeIds != null ? model.PartnerTypeIds.Split(",").ToList() : null;
            var partnerTypeIds = new List<byte>();
            if ( partnerType != null )
            {
                foreach ( var item in partnerType )
                {
                    var pId = Convert.ToByte(item);
                    partnerTypeIds.Add(pId);
                }
            }

            //Logo
            if ( model.Logo == null )
            {
                if ( !string.IsNullOrEmpty(dbPartner.Logo) && string.IsNullOrEmpty(model.LogoFileName) )
                {
                    await _s3BucketService.RemoveImageAsync(dbPartner.Logo);
                    logoUrl = string.Empty;
                    logoFileName = string.Empty;
                }
            }
            else
            {
                if ( !string.IsNullOrEmpty(dbPartner.Logo) )
                {
                    await _s3BucketService.RemoveImageAsync(dbPartner.Logo);
                }

                logoUrl = await _s3BucketService.UploadFileByTypeToStorageAsync(model.Logo , partnerId , EntityType.None , FileTypeEnum.Logo.ToString());
                logoFileName = model.Logo.FileName;
            }

            var partner = await EditPartner(new Partner
            {
                Id = partnerId ,
                Name = model.Name ,
                Description = !string.IsNullOrEmpty(model.Description)
                    ? model.Description
                    : string.Empty ,
                Logo = logoUrl ,
                LogoMetadata = logoFileName ,
                ShowOnPartnerPage = model.ShowOnPartnerPage ,
                IsArchived = model.IsArchived ,
                Link = model.Link
            } , partnerTypeIds);

            if ( partner == null )
            {
                throw new BusinessException("Can not edit the partner. Please, try again.");
            }

            return new PartnerResponseModel
            {
                Id = partner.Id ,
                Name = partner.Name ,
                Description = partner.Description ,
                ShowOnPartnerPage = partner.ShowOnPartnerPage ,
                IsArchived = partner.IsArchived ,
            };
        }

        public async Task ArchivePartnerAsync (int id)
        {
            var dbPartner = await _context.Partner.SingleOrDefaultAsync(t => t.Id == id);
            if ( dbPartner == null )
                throw new BusinessException("Partner not found");
            var mediaIds = _context.Media.Where(x => x.SourceId == id).ToList();
            var toolIds = await _context.Tool.Where(x => x.PartnerId == id).ToListAsync();

            // update user on DB
            await updateUserStatus(id);
            // update partner from DB
            await ArchivePartner(id);

            if (mediaIds.Count > 0 )
            {
                foreach ( var item in mediaIds )
                {
                    var media = _mediaCloudSearchProvider.GetCurrentItemFromCloud(item.Id.ToString() , 10 , 1).FirstOrDefault();
                    if ( media != null )
                    {
                        var clousearchdentity = new MediaCloudSearchEntity
                        {
                            Id = Convert.ToInt64(media.id) ,
                            Title = media.title ,
                            Description = media.description ,
                            LongDescription = media.longdescription ,
                            SeriesTitle = media.seriestitle ,
                            TopicTitle = media.topictitle ,
                            Tags = media.tags ,
                            Status = "Archived" ,//media.status,
                            MediaType = media.mediatype ,
                            Date = media.date ,
                            Source = media.source ,
                            UploadedBy = media.uploadedby ,
                            PublishedBy = media.publisedby ,
                            Logo = media.logo ,//featured image
                            ActiveFrom = media.activefrom ,
                            ActiveTo = media.activeto ,
                            IsPrivate = media.isprivate ,
                            thumbnail = media.thumbnail ,
                            seourl = media.seourl ,
                            LastUpdatedDate = media.lastupdateddate ,
                            UniqueId = media.uniqueid ,
                            IsSharingAllowed = media.issharingallowed ,
                            IsDeleted = media.isdeleted ,
                            IsVisibleOnGoogle = media.isvisibleongoogle ,
                        };
                        var _statusonCloud = _mediaCloudSearchProvider.UpdateToCloud(clousearchdentity);
                    }
                }
            }

            if ( toolIds.Count > 0 )
            {
                foreach ( var item in toolIds )
                {
                    var tool = _topicToolSeriesCloudProvider.GetCloudToolById(item.Id.ToString()).FirstOrDefault();
                    if ( tool != null )
                    {
                        var toolModel = new TopicToolSeriesModel
                        {
                            Id = item.Id ,
                            Title = tool.title ?? "" ,
                            Description = tool.description ?? "" ,
                            ParentTopic = "" ,
                            Logo = "" ,
                            SeoUrl = "" ,
                            FeaturedImage = tool.featuredimage ?? "" ,
                            Thumbnail = "" ,
                            AssignedTo = tool.assignedto ?? new List<string>() ,
                            DateCreated = tool.datecreated ,
                            Topics = tool.topics ?? new List<string>() ,
                            Link = tool.link ?? "" ,
                            ShowOnMenu = tool.showonmenu ,
                            ShowOnHomePage = tool.showonhomepage ,
                            ItemType = tool.itemtype ?? "" ,
                            Type = tool.type ?? "" ,
                            Partner = ""
                        };
                        var _statusonCloud = _topicToolSeriesCloudProvider.UpdateToCloud(toolModel);
                    }
                }
            }

        }

        #region // Private Methods
        private async Task<Partner> AddPartner (Partner entity , List<byte> partnerTypes)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                await _context.AddAsync(entity);
                await _context.SaveChangesAsync();

                var partnerId = entity.Id;
                var partnerPartnerType = new List<PartnerPartnerType>();
                foreach (var item in partnerTypes)
                {
                    partnerPartnerType.Add(new PartnerPartnerType { PartnerId = partnerId, PartnertypeId = item });
                }
                _context.PartnerPartnerType.AddRange(partnerPartnerType);
                await _context.SaveChangesAsync();

                transaction.Commit();
                return entity;
            }
            catch
            {
                transaction.Rollback();
            }
            return null;
        }

        private async Task UpdateLogoAsync (Partner parter , string logoUrl)
        {
            parter.Logo = logoUrl;
            _context.Update(parter);
            await _context.SaveChangesAsync();
        }

        private async Task<Partner> EditPartner (Partner entity , List<byte> partnerType)
        {
            var dbContextTransaction = _context.Database.BeginTransaction();
            using var transaction = dbContextTransaction;
            try
            {
                var dbPartner = _context.Partner.Include(y => y.PartnerPartnerType).SingleOrDefault(x => x.Id == entity.Id);
                if (dbPartner != null)
                {
                    dbPartner.Name = entity.Name;
                    dbPartner.Description = entity.Description;
                    dbPartner.Logo = entity.Logo;
                    dbPartner.LogoMetadata = entity.LogoMetadata;
                    dbPartner.IsArchived = entity.IsArchived;
                    dbPartner.Link = entity.Link;
                    dbPartner.ShowOnPartnerPage = entity.ShowOnPartnerPage;
                    _context.Partner.Update(dbPartner);
                    if (dbPartner.PartnerPartnerType != null)
                    {
                        _context.RemoveRange(dbPartner.PartnerPartnerType);
                    }
                    if (partnerType != null && partnerType.Count > 0)
                    {
                        var partnerPartnerType = new List<PartnerPartnerType>();
                        foreach (var item in partnerType)
                        {
                            partnerPartnerType.Add(new PartnerPartnerType { PartnerId = entity.Id, PartnertypeId = item });
                        }
                        _context.PartnerPartnerType.AddRange(partnerPartnerType);
                    }
                    await _context.SaveChangesAsync();

                    transaction.Commit();
                    return dbPartner;
                }
            }
            catch
            {
                transaction.Rollback();

            }
            return null;
        }

        private async Task RemovePartner (int partnerId)
        {
            var partner = await _context.Partner.SingleOrDefaultAsync(t => t.Id == partnerId);
            if ( partner != null )
            {
                using var transaction = _context.Database.BeginTransaction();
                try
                {
                    var mediaList = await _context.Media.Where(m => m.SourceId.HasValue && m.SourceId.Value == partnerId).ToListAsync();
                    mediaList.ForEach(x => x.MediastatusId = (int)MediaStatusEnum.Archived);
                    _context.Media.UpdateRange(mediaList);

                    var toolList = await _context.Tool.Where(x => x.PartnerId.HasValue && x.PartnerId == partnerId).ToListAsync();
                    toolList.ForEach(y => y.PartnerId = null);
                    _context.Tool.UpdateRange(toolList);

                    _context.Partner.Remove(partner);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
            }
        }

        private async Task updateUserStatus (int partnerId)
        {
            var userList = await _context.User.Where(m => m.PartnerId == partnerId).ToListAsync();
            if ( userList != null )
            {
                foreach ( var user in userList )
                {
                    user.UserstatusId = 3;
                    _context.User.Update(user);
                }
                await _context.SaveChangesAsync();
            }
        }

        private async Task ArchivePartner (int partnerId)
        {
            var partner = await _context.Partner.SingleOrDefaultAsync(t => t.Id == partnerId);
            if ( partner != null )
            {
                using var transaction = _context.Database.BeginTransaction();
                try
                {
                    var mediaList = await _context.Media.Where(m => m.SourceId.HasValue && m.SourceId.Value == partnerId).ToListAsync();
                    mediaList.ForEach(x => x.MediastatusId = (int)MediaStatusEnum.Archived);
                    _context.Media.UpdateRange(mediaList);

                    var toolList = await _context.Tool.Where(x => x.PartnerId.HasValue && x.PartnerId == partnerId).ToListAsync();
                    toolList.ForEach(y => y.PartnerId = null);
                    _context.Tool.UpdateRange(toolList);

                    partner.IsArchived = true;
                    _context.Partner.Update(partner);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
            }
        }
        #endregion
    }
}