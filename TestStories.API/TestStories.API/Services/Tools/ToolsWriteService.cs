using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestStories.API.Services;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.CloudSearch.Service.Interface;
using TestStories.Common;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;

namespace TestStories.API.Concrete
{
    public class ToolsWriteService : IToolsWriteService
    {
        readonly TestStoriesContext _context;
        readonly ICloudTopicToolSeriesProvider _topicToolSeriesCloudSearch;
        readonly IS3BucketService _s3BucketService;

        public ToolsWriteService(TestStoriesContext ctx, ICloudTopicToolSeriesProvider topicToolSeriesCloudSearch, IS3BucketService s3BucketService)
        {
            _context = ctx;
            _topicToolSeriesCloudSearch = topicToolSeriesCloudSearch;
            _s3BucketService = s3BucketService;
        }

        public async Task DeleteToolByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Series name cannot be empty");
            }

            var tool = _context.Tool.FirstOrDefault(x => x.Name.Trim() == name.Trim());
            if (tool == null)
            {
                throw new ArgumentException("Tool not found");
            }

            await RemoveToolAsync(tool.Id);
        }

        private async Task<Tool> AddToolAsync(AddToolModel model, string featuredImageMetaData)
        {
            var lstTopicIds = new List<int>();
            var lstSeriesIds = new List<int>();
            var lstMediaIds = new List<long>();
            var lstCombinedMediaIds = new List<long>();

            if (model.TopicIds != null)
            {
                lstTopicIds = model.TopicIds.Split(',').ToList().ConvertAll(int.Parse);
            }

            if (model.SeriesIds != null)
            {
                lstSeriesIds = model.SeriesIds.Split(',').ToList().ConvertAll(int.Parse);
            }

            if (model.MediaIds != null)
            {
                lstMediaIds = model.MediaIds.Split(',').ToList().ConvertAll(long.Parse);
            }

            var lstSeriesMediaIds = await _context.Media.Where(x => lstSeriesIds.Contains(x.SeriesId.Value)).Select(y => y.Id).ToListAsync();

            lstCombinedMediaIds = lstMediaIds.Union(lstSeriesMediaIds).ToList();

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var entity = new Tool
                    {
                        Name = model.Name,
                        Url = model.Link,
                        PartnerId = model.PartnerId.HasValue ? model.PartnerId : null,
                        TooltypeId = model.ToolTypeId.HasValue ? model.ToolTypeId : null,
                        Description = model.Description ?? string.Empty,
                        FeaturedImageMetadata= featuredImageMetaData,
                        ShowOnMenu = model.ShowOnMenu,
                        ShowOnHomepage = model.ShowOnHomePage
                    };
                    await _context.AddAsync(entity);
                    await _context.SaveChangesAsync();

                    if (model.MediaIds != null)
                    {
                        var lstToolMedia = new List<ToolMedia>();
                        foreach (var mediaId in lstMediaIds)
                        {
                            lstToolMedia.Add(new ToolMedia { MediaId = mediaId, ToolId = entity.Id });
                        }
                        _context.ToolMedia.AddRange(lstToolMedia);
                        await _context.SaveChangesAsync();

                    }
                    if (model.SeriesIds != null)
                    {
                        var lstToolSeries = new List<ToolSeries>();
                        foreach (var seriesId in lstSeriesIds)
                        {
                            lstToolSeries.Add(new ToolSeries { SeriesId = (int)seriesId, ToolId = entity.Id });
                        }
                        _context.ToolSeries.AddRange(lstToolSeries);
                        await _context.SaveChangesAsync();

                    }

                    if (model.TopicIds != null)
                    {
                        var lstToolTopic = new List<ToolTopic>();
                        foreach (var topicId in lstTopicIds)
                        {
                            lstToolTopic.Add(new ToolTopic { TopicId = (int)topicId, ToolId = entity.Id });
                        }
                        _context.ToolTopic.AddRange(lstToolTopic);
                        await _context.SaveChangesAsync();
                    }


                    // add resource reference to MediaResourceOrder table 
                    foreach ( var mediaId in lstCombinedMediaIds )
                    {
                        var mediaResource = await _context.MediaResourceOrder.Where(x => x.MediaId == mediaId).FirstOrDefaultAsync();
                        if ( mediaResource != null && !string.IsNullOrEmpty(mediaResource.ResourceIds.Trim()) )
                        {
                            //var lstResourceIds = mediaResource.ResourceIds.Split(",").ToList().ConvertAll(int.Parse);

                            var mos = 0;
                            var lstResourceIds = mediaResource.ResourceIds.Split(",").Where(m => int.TryParse(m , out mos)).Select(m => int.Parse(m)).ToList();
                           
                            lstResourceIds.Add(entity.Id);
                            mediaResource.ResourceIds = string.Join("," , lstResourceIds);
                            _context.MediaResourceOrder.Update(mediaResource);
                        }
                    }

                    await _context.SaveChangesAsync();
                    transaction.Commit();
                    return entity;
                }
                catch
                {
                    transaction.Rollback();
                }
            }
            return null;
        }

        private async Task UpdateAsync(Tool model)
        {
            _context.Update(model);
            await _context.SaveChangesAsync();
        }

        private async Task<Tool> EditToolAsync(EditToolModel model, string featuredImage, string featuredImageMetadata, List<int> lstTopicIds)
        {
            var lstSeriesIds = new List<int>();
            var lstMediaIds = new List<long>();


            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var dbTool = await _context.Tool.Include(x => x.ToolMedia).Include(y => y.ToolSeries).Include(z => z.ToolTopic).SingleOrDefaultAsync(t => t.Id == model.Id);
                    if (dbTool != null)
                    {
                        dbTool.Name = model.Name;
                        dbTool.PartnerId = model.PartnerId.HasValue ? model.PartnerId : null;
                        dbTool.TooltypeId = model.ToolTypeId.HasValue ? model.ToolTypeId : null;
                        dbTool.Description = model.Description ?? "";
                        dbTool.Url = model.Link ?? "";
                        dbTool.FeaturedImage = featuredImage;
                        dbTool.FeaturedImageMetadata = featuredImageMetadata;
                        dbTool.ShowOnMenu = model.ShowOnMenu;
                        dbTool.ShowOnHomepage = model.ShowOnHomePage;
                        _context.Tool.Update(dbTool);
                        await _context.SaveChangesAsync();

                        if (model.MediaIds != null)
                        {
                             lstMediaIds = model.MediaIds.Split(',').ToList().ConvertAll(long.Parse);

                            var lstRemovableMedia = dbTool.ToolMedia.Where(x => !lstMediaIds.Contains(x.MediaId)).ToList();
                            _context.ToolMedia.RemoveRange(lstRemovableMedia);
                            await _context.SaveChangesAsync();
                            var lstNewToolMedia = new List<ToolMedia>();
                            foreach (var mediaId in lstMediaIds)
                            {
                                var toolMedia = dbTool.ToolMedia.SingleOrDefault(x => x.MediaId == mediaId);
                                if (toolMedia == null)
                                {
                                    lstNewToolMedia.Add(new ToolMedia { ToolId = model.Id, MediaId = (long)mediaId });
                                }
                            }
                            _context.ToolMedia.AddRange(lstNewToolMedia);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            var lstRemovableMedia = dbTool.ToolMedia.Where(x => !lstMediaIds.Contains(x.MediaId)).ToList();
                            _context.ToolMedia.RemoveRange(lstRemovableMedia);
                            await _context.SaveChangesAsync();
                        }

                        // Add multiple topic

                        if (lstTopicIds != null)
                            {
                                var lstRemovableTopics = dbTool.ToolTopic.Where(x => !lstTopicIds.Contains(x.TopicId)).ToList();
                                _context.ToolTopic.RemoveRange(lstRemovableTopics);
                                await _context.SaveChangesAsync();
                                var lstNewToolTopic = new List<ToolTopic>();
                                foreach (var topicId in lstTopicIds)
                                {
                                    var toolTopic = dbTool.ToolTopic.SingleOrDefault(x => x.TopicId == topicId);
                                    if (toolTopic == null)
                                    {
                                        lstNewToolTopic.Add(new ToolTopic { ToolId = model.Id , TopicId = (int)topicId });
                                    }
                                }
                                _context.ToolTopic.AddRange(lstNewToolTopic);
                                await _context.SaveChangesAsync();
                            }


                            // seriesIds
                            if (model.SeriesIds != null)
                            {
                                lstSeriesIds = model.SeriesIds.Trim().Split(',').ToList().ConvertAll(int.Parse);

                                var lstRemovableSeries = dbTool.ToolSeries.Where(x => !lstSeriesIds.Contains(x.SeriesId)).ToList();
                                _context.ToolSeries.RemoveRange(lstRemovableSeries);
                                await _context.SaveChangesAsync();
                                var lstNewToolSeries = new List<ToolSeries>();
                                foreach (var seriesId in lstSeriesIds)
                                {
                                    var toolSeries = dbTool.ToolSeries.SingleOrDefault(x => x.SeriesId == seriesId);
                                    if (toolSeries == null)
                                    {
                                        lstNewToolSeries.Add(new ToolSeries { ToolId = model.Id, SeriesId = (int)seriesId });
                                    }
                                }
                                _context.ToolSeries.AddRange(lstNewToolSeries);
                                await _context.SaveChangesAsync();
                            }
                            else
                            {

                                var lstRemovableSeries = dbTool.ToolSeries.Where(x => !lstSeriesIds.Contains(x.SeriesId)).ToList();
                                _context.ToolSeries.RemoveRange(lstRemovableSeries);
                                await _context.SaveChangesAsync();
                            }


                        // add/remove the reference of the resource from the MediaResourceOrder table 
                        var removeableRsource = new List<MediaResourceOrder>();
                        var updatedMediaIds = await _context.ToolMedia.Where(x => x.ToolId == model.Id).Select(y => y.MediaId).ToListAsync();
                        var lstSeriesMediaIds = await _context.Media.Where(x => lstSeriesIds.Contains(x.SeriesId.Value)).Select(y => y.Id).ToListAsync();
                        updatedMediaIds = updatedMediaIds.Union(lstSeriesMediaIds).ToList();
                        if ( updatedMediaIds.Count > 0 )
                        {
                            // add resource reference to medias

                            var resourceMedias = await _context.MediaResourceOrder.Where(x => updatedMediaIds.Contains(x.MediaId)).ToListAsync();
                            var updatedResource = new List<MediaResourceOrder>();
                            foreach ( var item in resourceMedias )
                            {
                                if ( !string.IsNullOrEmpty(item.ResourceIds.Trim()) )
                                {
                                    var mos = 0;
                                    var lstResourceIds = item.ResourceIds.Split(",").Where(m => int.TryParse(m , out mos)).Select(m => int.Parse(m)).ToList();
                                    if ( !lstResourceIds.Contains(model.Id) )
                                    {
                                        lstResourceIds.Add(model.Id);
                                        item.ResourceIds = string.Join("," , lstResourceIds);
                                        updatedResource.Add(item);
                                    }
                                }
                            }
                            _context.MediaResourceOrder.UpdateRange(updatedResource);
                            await _context.SaveChangesAsync();
                        }

                        // remove the resource reference from the rest of the media

                        var removeableMediaResource = await _context.MediaResourceOrder.Where(x => !updatedMediaIds.Contains(x.MediaId)).ToListAsync();

                        foreach ( var item in removeableMediaResource )
                        {
                            if ( !string.IsNullOrEmpty(item.ResourceIds.Trim()) )
                            {
                                var mos = 0;
                                var lstResourceIds = item.ResourceIds.Split(",").Where(m => int.TryParse(m , out mos)).Select(m => int.Parse(m)).ToList();
                                if ( lstResourceIds.Contains(model.Id) )
                                {
                                    lstResourceIds.Remove(model.Id);
                                    item.ResourceIds = string.Join("," , lstResourceIds);
                                    removeableRsource.Add(item);
                                }
                            }
                        }
                        _context.MediaResourceOrder.UpdateRange(removeableRsource);
                        await _context.SaveChangesAsync();

                        transaction.Commit();
                            return dbTool;
                        }
                    }
                catch
                {
                    transaction.Rollback();
                }
            }

            return null;
        }

        public async Task RemoveToolAsync(int toolId)
        {

            var dbTool = await _context.Tool.SingleOrDefaultAsync(t => t.Id == toolId);
            if (dbTool == null)
                throw new BusinessException("Resource not found");
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                if (dbTool != null)
                {
                    _context.Tool.Remove(dbTool);
                    await _context.SaveChangesAsync();
                    var status = _topicToolSeriesCloudSearch.DeleteFromCloud("Tool" + toolId);

                    if (!string.IsNullOrEmpty(dbTool.FeaturedImage))
                    {
                        await _s3BucketService.RemoveImageAsync(dbTool.FeaturedImage);
                    }

                    // Remove the resource reference from the MediaResourceOrder table

                    var removeableResource = new List<MediaResourceOrder>();
                    var allMediaResource = await _context.MediaResourceOrder.ToListAsync();

                    foreach (var item in allMediaResource)
                    {
                        if (!string.IsNullOrEmpty(item.ResourceIds.Trim()))
                        {
                            var mos = 0;
                            var lstResourceIds = item.ResourceIds.Split(",").Where(m => int.TryParse(m, out mos)).Select(m => int.Parse(m)).ToList();
                            if (lstResourceIds.Contains(toolId))
                            {
                                lstResourceIds.Remove(toolId);
                                item.ResourceIds = string.Join(",", lstResourceIds);
                                removeableResource.Add(item);
                            }
                        }
                    }
                    _context.MediaResourceOrder.UpdateRange(removeableResource);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
            }
            catch
            {
                transaction.Rollback();
            }
        }

        public async Task<ToolViewModel> AddToolAsync (AddToolModel model)
        {
            var topics = new List<string>();
            var type = string.Empty;
            var partner = string.Empty;

            Tool tool;

            if ( model.Name != null )
            {
                var isToolExist = await _context.Tool.AnyAsync(x => x.Name == model.Name);
                if ( isToolExist )
                    throw new BusinessException("Name already exist try different name ");
            }
            if ( model.PartnerId != null )
            {
                var partnerDetail = await _context.Partner.FirstOrDefaultAsync(x => x.Id == model.PartnerId);
                if ( partnerDetail is null )
                {
                    throw new BusinessException("Invalid Partner");
                }
                partner = partnerDetail.Name;
            }

            if ( model.ToolTypeId != null )
            {
                var toolType = await _context.ToolType.FirstOrDefaultAsync(x => x.Id == model.ToolTypeId);
                if ( toolType is null )
                {
                    throw new BusinessException("Invalid Tool Type");
                }
                type = toolType.Name;
            }


            var featuredImageMetaData = model.FeaturedImage != null ? model.FeaturedImage.FileName : string.Empty;

            tool = await AddToolAsync(model , featuredImageMetaData);

            tool.FeaturedImage = model.FeaturedImage != null ? await _s3BucketService.UploadFileByTypeToStorageAsync(model.FeaturedImage , tool.Id , EntityType.Tools , FileTypeEnum.FeaturedImage.ToString()) : string.Empty;
            await UpdateAsync(tool);


            var mediaIds = ( from m in _context.ToolMedia.Where(t => t.ToolId == tool.Id)
                             select m.MediaId.ToString()
                                      ).ToList();

            var seriesNames = ( from series in _context.ToolSeries.Where(t => t.ToolId == tool.Id)
                                join seriesName in _context.Series
                                on series.SeriesId equals seriesName.Id
                                select seriesName.Name ).ToList();
            if ( model.TopicIds != null )
            {
                var lstTopicIds = new List<int>();
                if ( model.TopicIds != null )
                {
                    var tIds = model.TopicIds.Split(',');
                    foreach ( var tId in tIds )
                    {
                        lstTopicIds.Add(Convert.ToInt32(tId));
                    }
                }

                topics = await _context.Topic.Where(x => lstTopicIds.Contains(x.Id)).Select(y => y.Name).ToListAsync();
            }

            var toolModel = new TopicToolSeriesModel
            {
                Id = tool.Id ,
                Title = tool.Name ,
                Description = tool.Description ,
                ParentTopic = "" ,
                Logo = "" ,
                SeoUrl = "" ,
                FeaturedImage = tool.FeaturedImage ,
                BannerImage = "" ,
                Thumbnail = "" ,
                AssignedTo = mediaIds.Concat(seriesNames).ToList() ,
                DateCreated = tool.DateCreatedUtc.ToUniversalTime() ,
                Topics = topics ,
                Link = tool.Url ,
                ShowOnMenu = Convert.ToInt32(tool.ShowOnMenu) ,
                ItemType = "Tool" ,
                Type = type ,
                Partner = partner ,
                ShowOnHomePage = Convert.ToInt32(tool.ShowOnHomepage)
            };

            // Add to cloud
            var status = _topicToolSeriesCloudSearch.AddToCloud(toolModel);

            if ( status.ToLower().Trim() != CloudStatus.success.ToString() )
            {
               // _logger.LogError($"Add:Received: request for controller: ToolsController and action: AddToolAsync cloud status is: {status} and userId- is: {GetUserId()}");
            }


            if ( tool == null )
            {
                throw new BusinessException("Can not add a new tool. Please, try again.");
            }

            return new ToolViewModel
            {
                Id = tool.Id ,
                Name = tool.Name ,
                PartnerId = tool.PartnerId ,
                ToolTypeId = tool.TooltypeId ,
                Description = tool.Description ,
                Link = tool.Url
            };
        }

        public async Task<ToolViewModel> EditToolsAsync (EditToolModel model)
        {
            var lstTopicIds = new List<int>();
            var topicsName = new List<string>();
            var type = string.Empty;
            var partner = string.Empty;

            if ( model.TopicIds != null )
            {
                var tIds = model.TopicIds.Split(',');
                foreach ( var tId in tIds )
                {
                    lstTopicIds.Add(Convert.ToInt32(tId));
                }
            }

            var dbTool = await _context.Tool.SingleOrDefaultAsync(t => t.Id == model.Id);
            if ( dbTool == null )
                throw new BusinessException("Tool not found");


            if ( model.Name != null )
            {
                var toolDetail = await _context.Tool.SingleOrDefaultAsync(x => x.Name == model.Name);
                if ( toolDetail != null )
                    if ( toolDetail.Id != model.Id )
                       throw new BusinessException("Name already exist try different name");
            }

            if ( model.PartnerId != null )
            {
                var partnerDetail = await _context.Partner.FirstOrDefaultAsync(x => x.Id == model.PartnerId);
                if ( partnerDetail is null )
                {
                    throw new BusinessException("Invalid Partner");
                }
                partner = partnerDetail.Name;
            }

            if ( model.ToolTypeId != null )
            {
                var toolType = await _context.ToolType.FirstOrDefaultAsync(x => x.Id == model.ToolTypeId);
                if ( toolType is null )
                {
                    throw new BusinessException("Invalid Tool Type");
                }
                type = toolType.Name;
            }

            Tool tool;

            var featuredImageUrl = dbTool.FeaturedImage;
            var featuredImageFileName = dbTool.FeaturedImageMetadata;

            //FeaturedImage
            if ( model.FeaturedImage == null )
            {
                if ( !string.IsNullOrEmpty(dbTool.FeaturedImage) && string.IsNullOrEmpty(model.FeaturedImageFileName) )
                {
                    await _s3BucketService.RemoveImageAsync(dbTool.FeaturedImage);
                    featuredImageUrl = string.Empty;
                    featuredImageFileName = string.Empty;
                }
            }
            else
            {
                if ( !string.IsNullOrEmpty(dbTool.FeaturedImage) )
                {
                    await _s3BucketService.RemoveImageAsync(dbTool.FeaturedImage);
                }

                featuredImageUrl = await _s3BucketService.UploadFileByTypeToStorageAsync(model.FeaturedImage , model.Id , EntityType.Tools , FileTypeEnum.FeaturedImage.ToString());
                featuredImageFileName = model.FeaturedImage.FileName;
            }

            tool = await EditToolAsync(model , featuredImageUrl , featuredImageFileName , lstTopicIds);

            if ( tool == null )
            {
                throw new BusinessException("Can not edit tool. Please, try again.");
            }


            // update on cloud
            var mediaIds = ( from m in _context.ToolMedia.Where(t => t.ToolId == tool.Id)
                             select m.MediaId.ToString()
                                       ).ToList();
            var seriesNames = ( from series in _context.ToolSeries.Where(t => t.ToolId == tool.Id)
                                join seriesName in _context.Series
                                on series.SeriesId equals seriesName.Id
                                select seriesName.Name.ToString() ).ToList();

            if ( model.TopicIds != null )
            {
                if (lstTopicIds.Count > 0 )
                {
                    topicsName = await _context.Topic.Where(x => lstTopicIds.Contains(x.Id)).Select(y => y.Name).ToListAsync();
                }
            }

            var toolModel = new TopicToolSeriesModel
            {
                Id = tool.Id ,
                Title = tool.Name ,
                Description = tool.Description ,
                ParentTopic = "" ,
                Logo = "" ,
                SeoUrl = "" ,
                FeaturedImage = tool.FeaturedImage ,
                BannerImage = "" ,
                Thumbnail = "" ,
                AssignedTo = mediaIds.Concat(seriesNames).ToList() ,
                DateCreated = tool.DateCreatedUtc.ToUniversalTime() ,
                Topics = topicsName ,
                Link = tool.Url ,
                ShowOnMenu = Convert.ToInt32(tool.ShowOnMenu) ,
                ShowOnHomePage = Convert.ToInt32(tool.ShowOnHomepage) ,
                ItemType = "Tool" ,
                Type = type ,
                Partner = partner
            };

            // Add to cloud
            var status = _topicToolSeriesCloudSearch.AddToCloud(toolModel);

            if ( status.ToLower().Trim() != CloudStatus.success.ToString() )
            {
                //_logger.LogError($"Received: request for controller: ToolsController and action: EditToolsAsync input cloud status is:: {status} and userId- is: {GetUserId()}");
            }

            return new ToolViewModel
            {
                Id = tool.Id ,
                PartnerId = tool.PartnerId ,
                ToolTypeId = tool.TooltypeId ,
                Name = tool.Name ,
                Description = tool.Description ,
                Link = tool.Url ,
                ShowOnMenu = (bool)tool.ShowOnMenu ,
                ShowOnHomePage = Convert.ToBoolean(tool.ShowOnHomepage)
            };
        }

        public async Task<string> UpdateAllToolOnCloud ()
        {
            var updateStatus = string.Empty;
            // update on cloud
            var tools = ( from tool in _context.Tool.Include(x => x.Tooltype).Include(y => y.Partner)
                          let mediaIds = ( from m in _context.ToolMedia.Where(t => t.ToolId == tool.Id)
                                           select m.MediaId.ToString() )
                          let seriesNames = ( from series in _context.ToolSeries.Where(t => t.ToolId == tool.Id)
                                              join seriesName in _context.Series
                                              on series.SeriesId equals seriesName.Id
                                              select seriesName.Name )
                          let topicNames = ( from p in _context.ToolTopic.Where(x => x.ToolId == tool.Id)
                                             join q in _context.Topic
                                             on p.TopicId equals q.Id
                                             select q.Name )
                          select new TopicToolSeriesModel
                          {
                              Id = tool.Id ,
                              Title = tool.Name ,
                              Description = tool.Description ,
                              ParentTopic = "" ,
                              Logo = "" ,
                              SeoUrl = "" ,
                              FeaturedImage = tool.FeaturedImage ?? "" ,
                              BannerImage = "" ,
                              Thumbnail = "" ,
                              AssignedTo = mediaIds.Concat(seriesNames).ToList() ,
                              DateCreated = tool.DateCreatedUtc.ToUniversalTime() ,
                              Topics = topicNames.ToList() ,
                              Link = tool.Url ,
                              ShowOnMenu = Convert.ToInt32(tool.ShowOnMenu) ,
                              ShowOnHomePage = Convert.ToInt32(tool.ShowOnHomepage) ,
                              ItemType = "Tool" ,
                              Type = tool.Tooltype.Name ?? "" ,
                              Partner = tool.Partner.Name ?? ""
                          } ).ToList<dynamic>();
            _topicToolSeriesCloudSearch.BulkUpdateToCloud(tools);

            return updateStatus = "Updated";
        }

        public async Task MigrateDbToolsToCloud ()
        {
            var tools = ( from tool in _context.Tool.Include(x => x.Tooltype).Include(y => y.Partner)
                          let mediaIds = ( from m in _context.ToolMedia.Where(t => t.ToolId == tool.Id)
                                           select m.MediaId.ToString() )
                          let seriesNames = ( from series in _context.ToolSeries.Where(t => t.ToolId == tool.Id)
                                              join seriesName in _context.Series
                                              on series.SeriesId equals seriesName.Id
                                              select seriesName.Name )
                          let topicNames = ( from p in _context.ToolTopic.Where(x => x.ToolId == tool.Id)
                                             join q in _context.Topic
                                             on p.TopicId equals q.Id
                                             select q.Name )
                          select new TopicToolSeriesModel
                          {
                              Id = tool.Id ,
                              Title = tool.Name ,
                              Description = tool.Description ,
                              ParentTopic = "" ,
                              Logo = "" ,
                              SeoUrl = "" ,
                              FeaturedImage = tool.FeaturedImage ?? "" ,
                              BannerImage = "" ,
                              Thumbnail = "" ,
                              AssignedTo = mediaIds.Concat(seriesNames).ToList() ,
                              DateCreated = tool.DateCreatedUtc.ToUniversalTime() ,
                              Topics = topicNames.ToList() ,
                              Link = tool.Url ,
                              ShowOnMenu = Convert.ToInt32(tool.ShowOnMenu) ,
                              ShowOnHomePage = Convert.ToInt32(tool.ShowOnHomepage) ,
                              ItemType = "Tool" ,
                              Type = tool.Tooltype.Name ?? "" ,
                              Partner = tool.Partner.Name ?? ""
                          } ).ToList<dynamic>();

            _topicToolSeriesCloudSearch.BulkAddToCloud(tools);
        }
    }
}
