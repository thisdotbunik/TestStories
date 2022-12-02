using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestStories.API.Models.RequestModels;
using TestStories.API.Models.ResponseModels;
using TestStories.CloudSearch.Service.Interface;
using TestStories.Common;
using TestStories.DataAccess.Entities;

namespace TestStories.API.Services
{
    public class ToolTypeWriteService : IToolTypeWriteService
    {
        private readonly TestStoriesContext _context;
        private readonly ICloudTopicToolSeriesProvider _topicToolSeriesCloudSearch;
        public ToolTypeWriteService(TestStoriesContext ctx, ICloudTopicToolSeriesProvider topicToolSeriesCloudSearch)
        {
            _context = ctx;
            _topicToolSeriesCloudSearch = topicToolSeriesCloudSearch;
        }
           
        public async Task DeleteToolTypeByNameAsync(string name)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var toolType = await _context.ToolType.SingleOrDefaultAsync(x => x.Name.Trim() == name.Trim());
                if (toolType == null)
                {
                    throw new ArgumentException("Tool Type not found");
                }

                _context.ToolType.Remove(toolType);
                var tool = await _context.Tool.Where(x => x.TooltypeId == toolType.Id).ToListAsync();
                if (tool.Count > 0)
                {
                    tool.ForEach(x => x.TooltypeId = null);
                    _context.UpdateRange(tool);

                }

                transaction.Commit();
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> EnableToolType(int id)
        {
            var dbToolType = await _context.ToolType.SingleOrDefaultAsync(t => t.Id == id);
            if (dbToolType != null)
            {
                dbToolType.IsActive = true;
                _context.Update(dbToolType);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<ToolTypeModel> AddToolTypeAsync(AddToolType model)
        {
            ToolType toolType;


            if (model.Name != null)
            {
                var isTypeExist = await _context.ToolType.Where(x => x.Name.Trim() == model.Name.Trim()).AnyAsync();
                if (isTypeExist)
                {
                    throw new BusinessException("Name already exist try different name ");
                }
            }

            toolType = await AddToolType(new ToolType { Name = model.Name });

            if (toolType == null)
            {
                throw new BusinessException("Can not add a new tool type. Please, try again.");
            }

            return new ToolTypeModel
            {
                Id = toolType.Id,
                Name = toolType.Name,
                IsActive = toolType.IsActive,
            };
        }

        public async Task<ToolTypeModel> EditToolTypeAsync(int toolTypeId, AddToolType model)
        {
            var dbToolType = await _context.ToolType.SingleOrDefaultAsync(t => t.Id == toolTypeId);
            if (dbToolType == null)
                throw new BusinessException("ToolType not found");

            ToolType toolType;

            var toolTypeDetail = await _context.ToolType.SingleOrDefaultAsync(x => x.Name.Trim() == model.Name.Trim());
            if (toolTypeDetail != null)
            {
                if (toolTypeDetail.Id != toolTypeId)
                {
                    throw new BusinessException("Name already exist try different name");
                }
            }


            toolType = await EditToolType(toolTypeId, model);

            if (toolType == null)
            {
                throw new BusinessException("Can not add a new tool type. Please, try again.");
            }

            return new ToolTypeModel
            {
                Id = toolType.Id,
                Name = toolType.Name,
                IsActive = toolType.IsActive
            };
        }

        public async Task RemoveToolTypeAsync(int id)
        {
            var dbToolType = await _context.ToolType.SingleOrDefaultAsync(t => t.Id == id);
            if (dbToolType == null)
                throw new BusinessException("ToolType not found");

            var toolToUpdate = GetToolsByToolTypeId(id);
            await RemoveToolType(id);

            if (toolToUpdate.Count > 0)
            {
                _topicToolSeriesCloudSearch.BulkUpdateToCloud(toolToUpdate);
            }

        }

        public async Task EnableToolTypeAsync(int id)
        {
            var dbToolType = await _context.ToolType.SingleOrDefaultAsync(t => t.Id == id);
            if (dbToolType == null)
                throw new BusinessException("ToolType not found");

            var isUpdated = await EnableToolType(id);

            if (!isUpdated)
            {
                throw new BusinessException("Can not enable a tool type. Please, try again.");
            }
        }

        #region Private Methods

        private List<dynamic> GetToolsByToolTypeId(int toolTypeId)
        {
            var tools = (from tool in _context.Tool.ToList()
                         join tltyp in _context.ToolType on tool.TooltypeId equals tltyp.Id into tlypGroup
                         from toolType in tlypGroup.DefaultIfEmpty()
                         join prt in _context.Partner on tool.PartnerId equals prt.Id into prtGroup
                         from partner in prtGroup.DefaultIfEmpty()
                         let mediaIds = (from m in _context.ToolMedia.Where(t => t.ToolId == tool.Id)
                                         select m.MediaId.ToString()).ToList()
                         let seriesNames = (from series in _context.ToolSeries.Where(t => t.ToolId == tool.Id)
                                            join seriesName in _context.Series
                                                on series.SeriesId equals seriesName.Id
                                            select seriesName.Name).ToList()
                         let topicNames = (from p in _context.ToolTopic.Where(x => x.ToolId == tool.Id)
                                           join q in _context.Topic
                                               on p.TopicId equals q.Id
                                           select q.Name).ToList()
                         where tool.TooltypeId == toolTypeId
                         select new TopicToolSeriesModel
                         {
                             Id = tool.Id,
                             Title = tool.Name,
                             Description = tool.Description,
                             ParentTopic = "",
                             Logo = "",
                             SeoUrl = "",
                             FeaturedImage = tool.FeaturedImage ?? "",
                             Thumbnail = "",
                             AssignedTo = mediaIds.Concat(seriesNames).ToList(),
                             DateCreated = tool.DateCreatedUtc.ToUniversalTime(),
                             Topics = topicNames.ToList(),
                             Link = tool.Url,
                             ShowOnMenu = Convert.ToInt32(tool.ShowOnMenu),
                             ShowOnHomePage = Convert.ToInt32(tool.ShowOnHomepage),
                             ItemType = "Tool",
                             Type = "",
                             Partner = partner.Name ?? ""
                         }).ToList<dynamic>();
            return tools;
        }
        private async Task RemoveToolType(int typeId)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var dbToolType = await _context.ToolType.SingleOrDefaultAsync(t => t.Id == typeId);
                if (dbToolType != null)
                {
                    dbToolType.IsActive = false;
                    _context.Update(dbToolType);
                }
                var dbTool = await _context.Tool.Where(x => x.TooltypeId == typeId).ToListAsync();
                if (dbTool.Count > 0)
                {
                    dbTool.ForEach(x => x.TooltypeId = null);
                    _context.UpdateRange(dbTool);

                }

                transaction.Commit();
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
        private async Task<ToolType> AddToolType(ToolType entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        private async Task<ToolType> EditToolType(int toolTypeId, AddToolType entity)
        {
            var dbToolType = await _context.ToolType.SingleOrDefaultAsync(t => t.Id == toolTypeId);
            if (dbToolType != null)
            {
                dbToolType.Name = entity.Name;
                dbToolType.DateUpdatedUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return dbToolType;
            }
            return null;
        }

        #endregion
    }
}
